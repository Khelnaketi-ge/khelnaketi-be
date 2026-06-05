using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetMyProducts;

public sealed record GetMyProductsQuery : IRequest<List<ProductDto>>;

public sealed class GetMyProductsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<GetMyProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(
        GetMyProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var currentUserAccessLevel = await context.Users
            .Where(x => x.Id == currentUser.Id.Value)
            .Select(x => x.AccessLevel)
            .SingleOrDefaultAsync(cancellationToken);

        var isSuperAdmin = currentUserAccessLevel == AccessLevel.SuperAdmin;

        var products = await context.Products
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
            .OrderByDescending(x => x.Created)
            .ToListAsync(cancellationToken);
        
        using var scope = new MapContextScope();

        scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

        return mapper.Map<List<ProductDto>>(products);
    }
}
