using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.Catalog;

internal static class CatalogProductQueryBuilder
{
    private sealed record CategoryNode(int Id, int? ParentId);
    private sealed record AttributeSelection(int AttributeId, int OptionId);

    public static async Task<IQueryable<Product>> BuildAsync(
        IApplicationDbContext context,
        CatalogProductFilters filters,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var query = context.Products
            .AsNoTracking()
            .Where(x =>
                x.Status == ProductStatus.Active
                && x.Brand.Status == BrandStatus.Active);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var normalizedSearch = filters.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Translations.Any(t =>
                t.LanguageCode == languageCode
                && EF.Functions.Like(t.Title.ToLower(), $"%{normalizedSearch}%")));
        }

        if (filters.MinPrice is decimal minPrice)
        {
            query = query.Where(x => x.Price >= minPrice);
        }

        if (filters.MaxPrice is decimal maxPrice)
        {
            query = query.Where(x => x.Price <= maxPrice);
        }

        var categorySlugs = Normalize(filters.Categories);
        if (categorySlugs.Count > 0)
        {
            var categoryIds = await ResolveCategoryIdsAsync(
                context,
                categorySlugs,
                languageCode,
                cancellationToken);

            query = query.Where(x => categoryIds.Contains(x.CategoryId));
        }

        var brandSlugs = Normalize(filters.Brands);
        if (brandSlugs.Count > 0)
        {
            var brandIds = await context.Brands
                .AsNoTracking()
                .Where(x => x.Status == BrandStatus.Active && brandSlugs.Contains(x.Slug))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(x => brandIds.Contains(x.BrandId));
        }

        var attributeSelections = await ResolveAttributeSelectionsAsync(
            context,
            filters.Attributes,
            languageCode,
            cancellationToken);

        foreach (var group in attributeSelections.GroupBy(x => x.AttributeId))
        {
            var attributeId = group.Key;
            var optionIds = group.Select(x => x.OptionId).ToList();

            query = query.Where(x => x.AttributeValues.Any(value =>
                value.ProductAttributeId == attributeId
                && value.AttributeOptionId.HasValue
                && optionIds.Contains(value.AttributeOptionId.Value)));
        }

        return query;
    }

    private static async Task<HashSet<int>> ResolveCategoryIdsAsync(
        IApplicationDbContext context,
        IReadOnlyCollection<string> categorySlugs,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var selectedCategoryIds = await context.CategoryTranslations
            .AsNoTracking()
            .Where(x => x.LanguageCode == languageCode && categorySlugs.Contains(x.Slug))
            .Select(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        if (selectedCategoryIds.Count == 0)
        {
            return [];
        }

        var categories = await context.Categories
            .AsNoTracking()
            .Select(x => new CategoryNode(x.Id, x.ParentId))
            .ToListAsync(cancellationToken);
        var categoryIds = selectedCategoryIds.ToHashSet();
        var added = true;

        while (added)
        {
            added = false;

            foreach (var category in categories)
            {
                if (category.ParentId is int parentId
                    && categoryIds.Contains(parentId)
                    && categoryIds.Add(category.Id))
                {
                    added = true;
                }
            }
        }

        return categoryIds;
    }

    private static async Task<IReadOnlyCollection<AttributeSelection>> ResolveAttributeSelectionsAsync(
        IApplicationDbContext context,
        IReadOnlyCollection<string> attributes,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var parsedAttributes = Normalize(attributes)
            .Select(ParseAttributeFilter)
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .ToList();

        if (parsedAttributes.Count == 0)
        {
            return [];
        }

        var attributeSlugs = parsedAttributes.Select(x => x.AttributeSlug).ToHashSet();
        var optionSlugs = parsedAttributes.Select(x => x.OptionSlug).ToHashSet();
        var selections = await context.AttributeOptionTranslations
            .AsNoTracking()
            .Where(x =>
                x.LanguageCode == languageCode
                && optionSlugs.Contains(x.Slug)
                && x.AttributeOption.ProductAttribute.Translations.Any(t =>
                    t.LanguageCode == languageCode
                    && attributeSlugs.Contains(t.Slug)))
            .Select(x => new
            {
                AttributeId = x.AttributeOption.ProductAttributeId,
                AttributeSlug = x.AttributeOption.ProductAttribute.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Slug)
                    .First(),
                OptionId = x.AttributeOptionId,
                OptionSlug = x.Slug
            })
            .ToListAsync(cancellationToken);

        return selections
            .Where(x => parsedAttributes.Any(filter =>
                filter.AttributeSlug == x.AttributeSlug
                && filter.OptionSlug == x.OptionSlug))
            .Select(x => new AttributeSelection(x.AttributeId, x.OptionId))
            .ToList();
    }

    private static (string AttributeSlug, string OptionSlug)? ParseAttributeFilter(string value)
    {
        var separatorIndex = value.IndexOf(':');

        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            return null;
        }

        return (value[..separatorIndex], value[(separatorIndex + 1)..]);
    }

    private static HashSet<string> Normalize(IEnumerable<string> values) =>
        values
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();
}
