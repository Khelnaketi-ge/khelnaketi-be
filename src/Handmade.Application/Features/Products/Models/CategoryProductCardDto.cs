namespace Handmade.Application.Features.Products.Models;

public sealed record CategoryProductCardDto(
    int Id,
    string Title,
    string Slug,
    string CanonicalPath,
    decimal? Price,
    bool IsInStock,
    string? PrimaryImageUrl);
