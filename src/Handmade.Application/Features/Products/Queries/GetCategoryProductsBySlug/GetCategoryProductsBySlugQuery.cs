using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Handmade.Application.Features.Products.Queries.GetCategoryProductsBySlug;

public sealed record GetCategoryProductsBySlugQuery(string Slug)
    : IRequest<IReadOnlyCollection<CategoryProductCardDto>>;

public sealed class GetCategoryProductsBySlugQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    ICurrentLanguage currentLanguage)
    : IRequestHandler<GetCategoryProductsBySlugQuery, IReadOnlyCollection<CategoryProductCardDto>>
{
    private static readonly Regex CategorySlugWithIdRegex = new(@"^(?<slug>.+)-c(?<id>\d+)$", RegexOptions.Compiled);
    private sealed record CategoryNode(int Id, int? ParentId);

    public async Task<IReadOnlyCollection<CategoryProductCardDto>> Handle(
        GetCategoryProductsBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var slug = request.Slug.Trim();
        var categoryId = TryGetCategoryId(slug);

        if (categoryId == 0)
        {
            categoryId = await context.CategoryTranslations
                .AsNoTracking()
                .Where(x => x.LanguageCode == languageCode && x.Slug == slug)
                .Select(x => x.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (categoryId == 0)
        {
            return [];
        }

        var categories = await context.Categories
            .AsNoTracking()
            .Select(x => new CategoryNode(x.Id, x.ParentId))
            .ToListAsync(cancellationToken);
        var categoryIds = GetDescendantIds(categoryId, categories);

        var products = await context.Products
            .AsNoTracking()
            .Where(x =>
                categoryIds.Contains(x.CategoryId)
                && x.Status == ProductStatus.Active
                && x.Brand.Status == BrandStatus.Active)
            .Select(x => new
            {
                x.Id,
                x.Price,
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
            })
            .OrderByDescending(x => x.Id)
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

    private static HashSet<int> GetDescendantIds(int categoryId, IReadOnlyCollection<CategoryNode> categories)
    {
        var ids = new HashSet<int> { categoryId };
        var added = true;

        while (added)
        {
            added = false;

            foreach (var category in categories)
            {
                if (category.ParentId is int parentId && ids.Contains(parentId) && ids.Add(category.Id))
                {
                    added = true;
                }
            }
        }

        return ids;
    }

    private static int TryGetCategoryId(string slug)
    {
        var match = CategorySlugWithIdRegex.Match(slug);

        return match.Success && int.TryParse(match.Groups["id"].Value, out var categoryId)
            ? categoryId
            : 0;
    }
}
