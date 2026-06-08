using Handmade.Application.Features.Products.Models;
using Handmade.Application.Features.Products.Queries.Catalog;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetCatalogProducts;

public sealed record GetCatalogProductsQuery(CatalogProductFilters Filters)
    : IRequest<IReadOnlyCollection<CategoryProductCardDto>>;

public sealed class GetCatalogProductsQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    ICurrentLanguage currentLanguage)
    : IRequestHandler<GetCatalogProductsQuery, IReadOnlyCollection<CategoryProductCardDto>>
{
    public async Task<IReadOnlyCollection<CategoryProductCardDto>> Handle(
        GetCatalogProductsQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var query = await CatalogProductQueryBuilder.BuildAsync(
            context,
            request.Filters,
            languageCode,
            cancellationToken);

        var productCardsQuery = query
            .Select(x => new
            {
                x.Id,
                x.Price,
                x.Created,
                x.IsInStock,
                Translation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Title, t.Slug })
                    .FirstOrDefault(),
                CategoryTranslation = x.Category.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Slug })
                    .FirstOrDefault(),
                PrimaryImageObjectKey = x.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.Order)
                    .Select(image => image.Image.ObjectKey)
                    .FirstOrDefault()
            });

        productCardsQuery = request.Filters.SortBy switch
        {
            CatalogProductSort.PriceAscending => productCardsQuery
                .OrderBy(x => x.Price == null)
                .ThenBy(x => x.Price)
                .ThenByDescending(x => x.Created),
            CatalogProductSort.PriceDescending => productCardsQuery
                .OrderBy(x => x.Price == null)
                .ThenByDescending(x => x.Price)
                .ThenByDescending(x => x.Created),
            _ => productCardsQuery
                .OrderByDescending(x => x.Created)
                .ThenByDescending(x => x.Id)
        };

        var products = await productCardsQuery
            .ToListAsync(cancellationToken);

        return products
            .Where(x => x.Translation is not null && x.CategoryTranslation is not null)
            .Select(x => new CategoryProductCardDto(
                x.Id,
                x.Translation!.Title,
                x.Translation.Slug,
                $"/{languageCode}/{x.CategoryTranslation!.Slug}/{x.Translation.Slug}",
                x.Price,
                x.IsInStock,
                x.PrimaryImageObjectKey is null ? null : imageStorage.GetPublicUrl(x.PrimaryImageObjectKey)))
            .ToList();
    }
}
