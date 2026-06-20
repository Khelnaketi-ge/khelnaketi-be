using System.Text.RegularExpressions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailsDto?>;

public sealed class GetProductBySlugQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    ICurrentLanguage currentLanguage) : IRequestHandler<GetProductBySlugQuery, ProductDetailsDto?>
{
    private static readonly Regex ProductSlugWithIdRegex = new(
        @"^(?<slug>.+)-p(?<id>\d+)$",
        RegexOptions.Compiled);

    public async Task<ProductDetailsDto?> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = currentLanguage.Code;
        var match = ProductSlugWithIdRegex.Match(request.Slug.Trim());

        if (!match.Success || !int.TryParse(match.Groups["id"].Value, out var productId))
        {
            return null;
        }

        var item = await context.Products
            .AsNoTracking()
            .Where(x =>
                x.Id == productId
                && x.Status == ProductStatus.Active
                && x.Brand.Status == BrandStatus.Active)
            .Select(x => new
            {
                x.Id,
                x.Sku,
                x.Price,
                EffectivePrice = x.DiscountPrice ?? (x.DiscountPercent.HasValue && x.Price.HasValue
                    ? x.Price.Value - x.Price.Value * x.DiscountPercent.Value / 100
                    : x.Price),
                EffectiveDiscountPercent = x.DiscountPercent ?? (x.DiscountPrice.HasValue && x.Price.HasValue && x.Price.Value > 0
                    ? (x.Price.Value - x.DiscountPrice.Value) / x.Price.Value * 100
                    : (decimal?)null),
                x.StockQuantity,
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
                ImageObjectKeys = x.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.Order)
                    .Select(image => image.Image.ObjectKey)
                    .Take(5)
                    .ToList(),
                Attributes = x.AttributeValues
                    .Where(value => !value.ProductAttribute.IsDisabled)
                    .OrderBy(value => value.ProductAttribute.Translations
                        .Where(translation => translation.LanguageCode == languageCode)
                        .Select(translation => translation.Name)
                        .FirstOrDefault())
                    .Select(value => new
                    {
                        value.ProductAttributeId,
                        value.ProductAttribute.Type,
                        value.ProductAttribute.Unit,
                        Name = value.ProductAttribute.Translations
                            .Where(translation => translation.LanguageCode == languageCode)
                            .Select(translation => translation.Name)
                            .FirstOrDefault()
                            ?? value.ProductAttribute.Translations
                                .OrderBy(translation => translation.LanguageCode)
                                .Select(translation => translation.Name)
                                .FirstOrDefault(),
                        Value = value.AttributeOption == null
                            ? value.Value
                            : value.AttributeOption.Translations
                                .Where(translation => translation.LanguageCode == languageCode)
                                .Select(translation => translation.Value)
                                .FirstOrDefault()
                                ?? value.AttributeOption.Translations
                                    .OrderBy(translation => translation.LanguageCode)
                                    .Select(translation => translation.Value)
                                    .FirstOrDefault()
                                ?? value.Value
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var translation = item?.ProductTranslation;
        var categoryTranslation = item?.Category.Translation;

        if (item is null || translation is null || categoryTranslation is null)
        {
            return null;
        }

        var categorySlugsByLanguage = item.Category.Translations
            .ToDictionary(x => x.LanguageCode, x => x.Slug);
        var localizedPaths = item.ProductTranslations
            .Where(x => categorySlugsByLanguage.ContainsKey(x.LanguageCode))
            .ToDictionary(
                x => x.LanguageCode,
                x => $"/{x.LanguageCode}/{categorySlugsByLanguage[x.LanguageCode]}/{x.Slug}");
        var imageUrls = item.ImageObjectKeys
            .Where(objectKey => objectKey is not null)
            .Select(objectKey => imageStorage.GetPublicUrl(objectKey!))
            .Where(imageUrl => imageUrl is not null)
            .Select(imageUrl => imageUrl!)
            .ToList();

        return new ProductDetailsDto(
            item.Id,
            item.Sku,
            new ProductBrandSummaryDto(item.Brand.Id, item.Brand.Name, item.Brand.Slug),
            new ProductCategorySummaryDto(item.Category.Id, categoryTranslation.Name, categoryTranslation.Slug),
            translation.Title,
            translation.Slug,
            $"/{languageCode}/{categoryTranslation.Slug}/{translation.Slug}",
            localizedPaths,
            translation.ShortDescription,
            translation.Description,
            item.Price,
            item.EffectivePrice != item.Price ? item.EffectivePrice : null,
            item.EffectiveDiscountPercent,
            item.StockQuantity,
            item.Status,
            imageUrls.FirstOrDefault(),
            imageUrls,
            item.Attributes
                .Where(attribute => attribute.Name is not null)
                .Select(attribute => new ProductDetailAttributeDto(
                    attribute.ProductAttributeId,
                    attribute.Type,
                    attribute.Name!,
                    attribute.Value,
                    attribute.Unit))
                .ToList(),
            item.UpdatedAt);
    }
}
