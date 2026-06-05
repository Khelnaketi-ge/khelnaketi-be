using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetAdminProducts;

public sealed record GetAdminProductsQuery(
    int? BrandId,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IRequest<AdminProductsDto>;

public sealed record AdminProductsDto(
    IReadOnlyCollection<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed class GetAdminProductsQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<GetAdminProductsQuery, AdminProductsDto>
{
    public async Task<AdminProductsDto> Handle(
        GetAdminProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = request.Search?.Trim();

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
            .AsQueryable();

        if (request.BrandId.HasValue)
        {
            query = query.Where(x => x.BrandId == request.BrandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToUpperInvariant();
            query = query.Where(x =>
                x.Translations.Any(t => t.Title.ToUpper().Contains(normalizedSearch))
                || (x.Sku != null && x.Sku.ToUpper().Contains(normalizedSearch))
                || x.Brand.NormalizedName.Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderByDescending(x => x.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        using var scope = new MapContextScope();
        scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

        return new AdminProductsDto(mapper.Map<List<ProductDto>>(products), page, pageSize, totalCount);
    }
}
