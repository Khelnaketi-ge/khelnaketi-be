using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Seo.Queries.GetSeoProducts;

public sealed record GetSeoProductsQuery : IRequest<IReadOnlyCollection<SeoListingItemDto>>;

public sealed class GetSeoProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSeoProductsQuery, IReadOnlyCollection<SeoListingItemDto>>
{
    public async Task<IReadOnlyCollection<SeoListingItemDto>> Handle(
        GetSeoProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProductTranslations
            .AsNoTracking()
            .Where(x => x.Product.Status == ProductStatus.Active && x.Product.Brand.Status == BrandStatus.Active)
            .Select(x => new SeoListingItemDto(
                x.LanguageCode,
                x.Slug,
                x.Product.Updated ?? x.Product.Created,
                true))
            .ToListAsync(cancellationToken);
    }
}
