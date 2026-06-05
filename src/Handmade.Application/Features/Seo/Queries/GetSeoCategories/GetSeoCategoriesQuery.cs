using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Seo.Queries.GetSeoCategories;

public sealed record GetSeoCategoriesQuery : IRequest<IReadOnlyCollection<SeoListingItemDto>>;

public sealed class GetSeoCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSeoCategoriesQuery, IReadOnlyCollection<SeoListingItemDto>>
{
    public async Task<IReadOnlyCollection<SeoListingItemDto>> Handle(
        GetSeoCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CategoryTranslations
            .AsNoTracking()
            .Select(x => new SeoListingItemDto(
                x.LanguageCode,
                x.Slug,
                x.Category.Updated ?? x.Category.Created,
                true))
            .ToListAsync(cancellationToken);
    }
}
