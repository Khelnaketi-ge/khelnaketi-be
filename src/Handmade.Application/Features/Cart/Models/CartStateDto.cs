namespace Handmade.Application.Features.Cart.Models;

public sealed record CartStateDto(int ProductId, bool IsInCart);

public sealed record CartItemDto(
    int Id,
    int ProductId,
    int Quantity,
    string Title,
    string Slug,
    string CanonicalPath,
    decimal? Price,
    bool IsInStock,
    string? PrimaryImageUrl);
