namespace Handmade.Application.Features.Cart.Models;

public sealed record CartStateDto(int ProductId, bool IsInCart, string? Reason = null);

public sealed record CartSummaryDto(
    int TotalItems,
    IReadOnlyCollection<int> ProductIds);

public sealed record CartItemDto(
    int Id,
    int ProductId,
    int Quantity,
    string Title,
    string Slug,
    string CanonicalPath,
    decimal? Price,
    decimal? OriginalPrice,
    decimal? DiscountPercent,
    int StockQuantity,
    string? PrimaryImageUrl);
