using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetMyProducts;

public sealed record GetMyProductsQuery(
    ProductStatus? Status,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IRequest<MyProductsDto>;

public sealed record MyProductsDto(
    IReadOnlyCollection<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class GetMyProductsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<GetMyProductsQuery, MyProductsDto>
{
    public async Task<MyProductsDto> Handle(
        GetMyProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = request.Search?.Trim();

        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var currentUserAccessLevel = await context.Users
            .Where(x => x.Id == currentUser.Id.Value)
            .Select(x => x.AccessLevel)
            .SingleOrDefaultAsync(cancellationToken);

        var isSuperAdmin = currentUserAccessLevel == AccessLevel.SuperAdmin;

        var query = context.Products
            .AsNoTracking()
            .Include(x => x.Brand)
            .Include(x => x.Category)
                .ThenInclude(x => x.Translations)
            .Include(x => x.Translations)
            .Include(x => x.Images)
                .ThenInclude(x => x.Image)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Translations)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
                    .ThenInclude(x => x!.Translations)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
            .Where(x => isSuperAdmin || x.Brand.OwnerUserId == currentUser.Id.Value)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToUpperInvariant();
            query = query.Where(x =>
                x.Translations.Any(t => t.Title.ToUpper().Contains(normalizedSearch))
                || (x.Sku != null && x.Sku.ToUpper().Contains(normalizedSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderByDescending(x => x.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        using var scope = new MapContextScope();

        scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

        return new MyProductsDto(mapper.Map<List<ProductDto>>(products), page, pageSize, totalCount);
    }
}
