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
                x.DiscountPrice,
                x.DiscountPercent,
                EffectivePrice = x.DiscountPrice ?? (x.DiscountPercent.HasValue && x.Price.HasValue
                    ? x.Price.Value - x.Price.Value * x.DiscountPercent.Value / 100
                    : x.Price),
                EffectiveDiscountPercent = x.DiscountPercent ?? (x.DiscountPrice.HasValue && x.Price.HasValue && x.Price.Value > 0
                    ? (x.Price.Value - x.DiscountPrice.Value) / x.Price.Value * 100
                    : (decimal?)null),
                x.Created,
                x.StockQuantity,
                Translation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Title, t.Slug })
                    .FirstOrDefault(),
                CategoryTranslation = x.Category.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Slug })
                    .FirstOrDefault(),
                ImageObjectKeys = x.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.Order)
                    .Select(image => image.Image.ObjectKey)
                    .ToList()
            });

        productCardsQuery = request.Filters.SortBy switch
        {
            CatalogProductSort.PriceAscending => productCardsQuery
                .OrderBy(x => x.EffectivePrice == null)
                .ThenBy(x => x.EffectivePrice)
                .ThenByDescending(x => x.Created),
            CatalogProductSort.PriceDescending => productCardsQuery
                .OrderBy(x => x.EffectivePrice == null)
                .ThenByDescending(x => x.EffectivePrice)
                .ThenByDescending(x => x.Created),
            CatalogProductSort.DiscountDescending => productCardsQuery
                .OrderBy(x => x.EffectiveDiscountPercent == null)
                .ThenByDescending(x => x.EffectiveDiscountPercent)
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
                x.EffectivePrice != x.Price ? x.EffectivePrice : null,
                x.EffectiveDiscountPercent,
                x.StockQuantity,
                x.ImageObjectKeys.FirstOrDefault() is string primaryImageObjectKey
                    ? imageStorage.GetPublicUrl(primaryImageObjectKey)
                    : null,
                x.ImageObjectKeys
                    .Select(imageStorage.GetPublicUrl)
                    .OfType<string>()
                    .ToList()))
            .ToList();
    }
}
