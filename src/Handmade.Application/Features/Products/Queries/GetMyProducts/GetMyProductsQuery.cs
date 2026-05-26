using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetMyProducts;

public sealed record GetMyProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

public sealed class GetMyProductsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage) : IRequestHandler<GetMyProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(
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
            .Include(x => x.Category)
            .Include(x => x.Images)
                .ThenInclude(x => x.Image)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.ProductAttribute)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
            .Where(x => isSuperAdmin || x.Brand.OwnerUserId == currentUser.Id.Value)
            .OrderByDescending(x => x.Created)
            .ToListAsync(cancellationToken);

        return products.Select(product => ProductMappings.ToDto(product, imageStorage)).ToList();
    }
}
