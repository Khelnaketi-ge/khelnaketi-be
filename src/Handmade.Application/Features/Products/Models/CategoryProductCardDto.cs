namespace Handmade.Application.Features.Products.Models;

public sealed record CategoryProductCardDto(
    int Id,
    string Title,
    string Slug,
    string CanonicalPath,
    decimal? Price,
    decimal? DiscountPrice,
    decimal? DiscountPercent,
    bool IsInStock,
    string? PrimaryImageUrl,
    IReadOnlyCollection<string> ImageUrls);
