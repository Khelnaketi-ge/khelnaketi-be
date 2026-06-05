using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Handmade.Application.Features.Seo.Queries.GetCategoryBySlug;

public sealed record GetCategoryBySlugQuery(string LanguageCode, string Slug) : IRequest<CategorySeoDto?>;

public sealed class GetCategoryBySlugQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoryBySlugQuery, CategorySeoDto?>
{
    private static readonly Regex CategorySlugWithIdRegex = new(@"^(?<slug>.+)-c(?<id>\d+)$", RegexOptions.Compiled);

    public async Task<CategorySeoDto?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCodes.Normalize(request.LanguageCode);
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
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var translation = item?.Translation;

        return item is null || translation is null
            ? null
            : new CategorySeoDto(
                item.Id,
                item.ParentId,
                translation.Name,
                translation.Slug,
                $"/{languageCode}/category/{translation.Slug}",
                item.UpdatedAt);
    }

    private static int TryGetCategoryId(string slug)
    {
        var match = CategorySlugWithIdRegex.Match(slug);

        return match.Success && int.TryParse(match.Groups["id"].Value, out var categoryId)
            ? categoryId
            : 0;
    }
}
