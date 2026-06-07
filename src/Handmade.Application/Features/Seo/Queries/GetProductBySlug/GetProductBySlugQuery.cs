using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Handmade.Application.Features.Seo.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductSeoDto?>;

public sealed class GetProductBySlugQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    ICurrentLanguage currentLanguage) : IRequestHandler<GetProductBySlugQuery, ProductSeoDto?>
{
    private static readonly Regex ProductSlugWithIdRegex = new(@"^(?<slug>.+)-p(?<id>\d+)$", RegexOptions.Compiled);

    public async Task<ProductSeoDto?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var slug = request.Slug.Trim();

        var match = ProductSlugWithIdRegex.Match(slug);
        if (!match.Success || !int.TryParse(match.Groups["id"].Value, out var productId))
        {
            return null;
        }

        var item = await context.Products
            .AsNoTracking()
            .Where(x => x.Id == productId && x.Status == ProductStatus.Active && x.Brand.Status == BrandStatus.Active)
            .Select(x => new
            {
                x.Id,
                x.Price,
                x.IsInStock,
                x.Status,
                UpdatedAt = x.Updated ?? x.Created,
                ProductTranslation = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Title, t.Slug, t.ShortDescription, t.Description })
                    .FirstOrDefault(),
                ProductTranslations = x.Translations
                    .Select(t => new { t.LanguageCode, t.Slug })
                    .ToList(),
                Brand = new
                {
                    x.Brand.Id,
                    x.Brand.Name,
                    x.Brand.Slug
                },
                Category = new
                {
                    x.Category.Id,
                    Translation = x.Category.Translations
                        .Where(t => t.LanguageCode == languageCode)
                        .Select(t => new { t.Name, t.Slug })
                        .FirstOrDefault(),
                    Translations = x.Category.Translations
                        .Select(t => new { t.LanguageCode, t.Slug })
                        .ToList()
                },
                PrimaryImageObjectKey = x.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.Order)
                    .Select(image => image.Image.ObjectKey)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var translation = item?.ProductTranslation;
        var categoryTranslation = item?.Category.Translation;

        if (item is null || translation is null || categoryTranslation is null)
        {
            return null;
        }

        var categorySlugsByLanguage = item.Category.Translations.ToDictionary(x => x.LanguageCode, x => x.Slug);
        var localizedPaths = item.ProductTranslations
            .Where(x => categorySlugsByLanguage.ContainsKey(x.LanguageCode))
            .ToDictionary(
                x => x.LanguageCode,
                x => $"/{x.LanguageCode}/{categorySlugsByLanguage[x.LanguageCode]}/{x.Slug}");

        return new ProductSeoDto(
            item.Id,
            new SeoBrandSummaryDto(item.Brand.Id, item.Brand.Name, item.Brand.Slug),
            new SeoCategorySummaryDto(item.Category.Id, categoryTranslation.Name, categoryTranslation.Slug),
            translation.Title,
            translation.Slug,
            $"/{languageCode}/{categoryTranslation.Slug}/{translation.Slug}",
            localizedPaths,
            translation.ShortDescription,
            translation.Description,
            item.Price,
            item.IsInStock,
            item.Status,
            item.PrimaryImageObjectKey is null ? null : imageStorage.GetPublicUrl(item.PrimaryImageObjectKey),
            item.UpdatedAt);
    }
}
