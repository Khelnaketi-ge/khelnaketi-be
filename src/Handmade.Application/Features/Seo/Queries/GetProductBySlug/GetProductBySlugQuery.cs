using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Handmade.Application.Features.Seo.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string LanguageCode, string Slug) : IRequest<ProductSeoDto?>;

public sealed class GetProductBySlugQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage) : IRequestHandler<GetProductBySlugQuery, ProductSeoDto?>
{
    private static readonly Regex ProductSlugWithIdRegex = new(@"^(?<slug>.+)-p(?<id>\d+)$", RegexOptions.Compiled);

    public async Task<ProductSeoDto?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCodes.Normalize(request.LanguageCode);
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
                        .FirstOrDefault()
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

        if (item is null || translation is null || categoryTranslation is null || translation.Slug != slug)
        {
            return null;
        }

        return new ProductSeoDto(
            item.Id,
            new SeoBrandSummaryDto(item.Brand.Id, item.Brand.Name, item.Brand.Slug),
            new SeoCategorySummaryDto(item.Category.Id, categoryTranslation.Name, categoryTranslation.Slug),
            translation.Title,
            translation.Slug,
            $"/{languageCode}/products/{translation.Slug}",
            translation.ShortDescription,
            translation.Description,
            item.Price,
            item.IsInStock,
            item.Status,
            item.PrimaryImageObjectKey is null ? null : imageStorage.GetPublicUrl(item.PrimaryImageObjectKey),
            item.UpdatedAt);
    }
}
