using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Seo.Queries.GetSeoBrands;

public sealed record GetSeoBrandsQuery : IRequest<IReadOnlyCollection<SeoListingItemDto>>;

public sealed class GetSeoBrandsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSeoBrandsQuery, IReadOnlyCollection<SeoListingItemDto>>
{
    public async Task<IReadOnlyCollection<SeoListingItemDto>> Handle(
        GetSeoBrandsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Brands
            .AsNoTracking()
            .Where(x => x.Status == BrandStatus.Active)
            .Select(x => new SeoListingItemDto(
                "en",
                x.Slug,
                x.Updated ?? x.Created,
                true))
            .ToListAsync(cancellationToken);
    }
}
