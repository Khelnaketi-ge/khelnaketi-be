using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Handmade.Application.Features.Seo.Queries.GetCategoryBySlug;

public sealed record GetCategoryBySlugQuery(string Slug) : IRequest<CategorySeoDto?>;

public sealed class GetCategoryBySlugQueryHandler(
    IApplicationDbContext context,
    ICurrentLanguage currentLanguage)
    : IRequestHandler<GetCategoryBySlugQuery, CategorySeoDto?>
{
    private static readonly Regex CategorySlugWithIdRegex = new(@"^(?<slug>.+)-c(?<id>\d+)$", RegexOptions.Compiled);
    private sealed record CategoryBreadcrumbNode(
        int Id,
        int? ParentId,
        string? Name,
        string? Slug);

    public async Task<CategorySeoDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var slug = request.Slug.Trim();

        var categoryId = TryGetCategoryId(slug);

        if (categoryId == 0)
        {
            categoryId = await context.CategoryTranslations
                .AsNoTracking()
                .Where(x => x.Slug == slug)
                .OrderByDescending(x => x.LanguageCode == languageCode)
                .Select(x => x.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (categoryId == 0)
        {
            return null;
        }

        var item = await context.Categories
            .AsNoTracking()
            .Where(x => x.Id == categoryId)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                UpdatedAt = x.Updated ?? x.Created,
                Translation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Name, t.Slug })
                    .FirstOrDefault(),
                Translations = x.Translations
                    .Select(t => new { t.LanguageCode, t.Slug })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var translation = item?.Translation;

        if (item is null || translation is null)
        {
            return null;
        }

        var categoryNodes = await context.Categories
            .AsNoTracking()
            .Select(x => new CategoryBreadcrumbNode(
                x.Id,
                x.ParentId,
                x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Name)
                    .FirstOrDefault(),
                x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Slug)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
        var breadcrumbs = GetBreadcrumbs(item.Id, languageCode, categoryNodes);

        return new CategorySeoDto(
            item.Id,
            item.ParentId,
            translation.Name,
            translation.Slug,
            BuildCategoryCatalogPath(languageCode, translation.Slug),
            breadcrumbs,
            item.Translations.ToDictionary(
                x => x.LanguageCode,
                x => BuildCategoryCatalogPath(x.LanguageCode, x.Slug)),
            item.UpdatedAt);
    }

    private static IReadOnlyCollection<SeoBreadcrumbDto> GetBreadcrumbs(
        int categoryId,
        string languageCode,
        IReadOnlyCollection<CategoryBreadcrumbNode> categories)
    {
        var categoryById = categories.ToDictionary(x => x.Id);
        var nodes = new Stack<CategoryBreadcrumbNode>();
        var visitedIds = new HashSet<int>();
        var currentCategoryId = categoryId;

        while (categoryById.TryGetValue(currentCategoryId, out var category)
               && visitedIds.Add(currentCategoryId))
        {
            nodes.Push(category);

            if (category.ParentId is not int parentId)
            {
                break;
            }

            currentCategoryId = parentId;
        }

        return nodes
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Slug))
            .Select(x => new SeoBreadcrumbDto(x.Name!, BuildCategoryCatalogPath(languageCode, x.Slug!)))
            .ToList();
    }

    private static string BuildCategoryCatalogPath(string languageCode, string slug) =>
        $"/{languageCode}/catalog?category={Uri.EscapeDataString(slug)}";

    private static int TryGetCategoryId(string slug)
    {
        var match = CategorySlugWithIdRegex.Match(slug);

        return match.Success && int.TryParse(match.Groups["id"].Value, out var categoryId)
            ? categoryId
            : 0;
    }
}
