using Handmade.Application.Features.Products.Queries.Catalog;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetCatalogFilters;

public sealed record GetCatalogFiltersQuery(CatalogProductFilters Filters) : IRequest<CatalogFiltersDto>;

public sealed class GetCatalogFiltersQueryHandler(
    IApplicationDbContext context,
    ICurrentLanguage currentLanguage)
    : IRequestHandler<GetCatalogFiltersQuery, CatalogFiltersDto>
{
    public async Task<CatalogFiltersDto> Handle(
        GetCatalogFiltersQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var query = await CatalogProductQueryBuilder.BuildAsync(
            context,
            request.Filters,
            languageCode,
            cancellationToken);
        var categoryQuery = await CatalogProductQueryBuilder.BuildAsync(
            context,
            request.Filters with { Categories = [] },
            languageCode,
            cancellationToken);
        var brandQuery = await CatalogProductQueryBuilder.BuildAsync(
            context,
            request.Filters with { Brands = [] },
            languageCode,
            cancellationToken);
        var attributeQuery = await CatalogProductQueryBuilder.BuildAsync(
            context,
            request.Filters with { Attributes = [] },
            languageCode,
            cancellationToken);

        var priceBounds = await query
            .Where(x => x.Price.HasValue)
            .GroupBy(_ => 1)
            .Select(x => new
            {
                Min = x.Min(product => product.Price),
                Max = x.Max(product => product.Price)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var categoryRows = await categoryQuery
            .GroupBy(x => x.CategoryId)
            .Select(x => new { CategoryId = x.Key, Count = x.Count() })
            .Join(
                context.CategoryTranslations.AsNoTracking().Where(x => x.LanguageCode == languageCode),
                count => count.CategoryId,
                translation => translation.CategoryId,
                (count, translation) => new
                {
                    translation.Name,
                    translation.Slug,
                    count.Count
                })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        var categories = categoryRows
            .Select(x => new CatalogFilterOptionDto(x.Name, x.Slug, x.Count))
            .ToList();

        var brandRows = await brandQuery
            .GroupBy(x => x.BrandId)
            .Select(x => new { BrandId = x.Key, Count = x.Count() })
            .Join(
                context.Brands.AsNoTracking().Where(x => x.Status == BrandStatus.Active),
                count => count.BrandId,
                brand => brand.Id,
                (count, brand) => new
                {
                    brand.Name,
                    brand.Slug,
                    count.Count
                })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        var brands = brandRows
            .Select(x => new CatalogFilterOptionDto(x.Name, x.Slug, x.Count))
            .ToList();

        var optionCounts = await attributeQuery
            .SelectMany(product => product.AttributeValues
                .Where(value =>
                    value.AttributeOptionId.HasValue
                    && !value.ProductAttribute.IsDisabled
                    && value.ProductAttribute.CategoryAttributes.Any(categoryAttribute =>
                        categoryAttribute.IsFilterable
                        && categoryAttribute.CategoryId == product.CategoryId))
                .Select(value => new
                {
                    value.ProductAttributeId,
                    AttributeOptionId = value.AttributeOptionId!.Value
                }))
            .GroupBy(x => new { x.ProductAttributeId, x.AttributeOptionId })
            .Select(x => new
            {
                x.Key.ProductAttributeId,
                x.Key.AttributeOptionId,
                Count = x.Count()
            })
            .ToListAsync(cancellationToken);

        var attributeIds = optionCounts.Select(x => x.ProductAttributeId).ToHashSet();
        var optionIds = optionCounts.Select(x => x.AttributeOptionId).ToHashSet();
        var attributes = await context.ProductAttributes
            .AsNoTracking()
            .Where(x => attributeIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                Translation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Name, t.Slug })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
        var options = await context.AttributeOptions
            .AsNoTracking()
            .Where(x => optionIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.ProductAttributeId,
                x.Order,
                Translation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Value, t.Slug })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
        var countByOptionId = optionCounts.ToDictionary(x => x.AttributeOptionId, x => x.Count);
        var attributesById = attributes
            .Where(x => x.Translation is not null)
            .ToDictionary(x => x.Id);
        var attributeFilters = options
            .Where(x => x.Translation is not null && attributesById.ContainsKey(x.ProductAttributeId))
            .GroupBy(x => x.ProductAttributeId)
            .Select(group =>
            {
                var attribute = attributesById[group.Key];

                return new CatalogAttributeFilterDto(
                    attribute.Translation!.Name,
                    attribute.Translation.Slug,
                    group
                        .OrderBy(x => x.Order)
                        .ThenBy(x => x.Translation!.Value)
                        .Select(x => new CatalogFilterOptionDto(
                            x.Translation!.Value,
                            x.Translation.Slug,
                            countByOptionId[x.Id]))
                        .ToList());
            })
            .OrderBy(x => x.Label)
            .ToList();

        return new CatalogFiltersDto(
            priceBounds?.Min,
            priceBounds?.Max,
            categories,
            attributeFilters,
            brands);
    }
}
