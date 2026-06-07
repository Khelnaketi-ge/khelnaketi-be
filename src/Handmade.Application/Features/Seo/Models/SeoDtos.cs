using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Seo.Models;

public sealed record SeoBrandSummaryDto(
    int Id,
    string DisplayName,
    string Slug);

public sealed record SeoCategorySummaryDto(
    int Id,
    string Name,
    string Slug);

public sealed record SeoBreadcrumbDto(
    string Label,
    string Href);

public sealed record ProductSeoDto(
    int Id,
    SeoBrandSummaryDto Brand,
    SeoCategorySummaryDto Category,
    string Title,
    string Slug,
    string CanonicalPath,
    IReadOnlyDictionary<string, string> LocalizedPaths,
    string? ShortDescription,
    string? Description,
    decimal? Price,
    bool IsInStock,
    ProductStatus Availability,
    string? PrimaryImageUrl,
    DateTimeOffset UpdatedAt);

public sealed record CategorySeoDto(
    int Id,
    int? ParentId,
    string Name,
    string Slug,
    string CanonicalPath,
    IReadOnlyCollection<SeoBreadcrumbDto> Breadcrumbs,
    IReadOnlyDictionary<string, string> LocalizedPaths,
    DateTimeOffset UpdatedAt);

public sealed record BrandSeoDto(
    int Id,
    string DisplayName,
    string Slug,
    string CanonicalPath,
    string? Description,
    string? LogoUrl,
    DateTimeOffset UpdatedAt);

public sealed record SeoListingItemDto(
    string LanguageCode,
    string Slug,
    DateTimeOffset UpdatedAt,
    bool IsActive);
