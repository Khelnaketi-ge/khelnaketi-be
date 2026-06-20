using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Products.Models;

public sealed record ProductBrandSummaryDto(
    int Id,
    string DisplayName,
    string Slug);

public sealed record ProductCategorySummaryDto(
    int Id,
    string Name,
    string Slug);

public sealed record ProductDetailAttributeDto(
    int Id,
    AttributeType Type,
    string Name,
    string Value,
    string? Unit);

public sealed record ProductDetailsDto(
    int Id,
    string? Sku,
    ProductBrandSummaryDto Brand,
    ProductCategorySummaryDto Category,
    string Title,
    string Slug,
    string CanonicalPath,
    IReadOnlyDictionary<string, string> LocalizedPaths,
    string? ShortDescription,
    string? Description,
    decimal? Price,
    decimal? DiscountPrice,
    decimal? DiscountPercent,
    int StockQuantity,
    ProductStatus Availability,
    string? PrimaryImageUrl,
    IReadOnlyCollection<string> ImageUrls,
    IReadOnlyCollection<ProductDetailAttributeDto> Attributes,
    DateTimeOffset UpdatedAt);
