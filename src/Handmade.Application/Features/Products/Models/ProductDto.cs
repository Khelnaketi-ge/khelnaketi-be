using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Products.Models;

public sealed record ProductDto(
    int Id,
    int BrandId,
    int CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string? Sku,
    decimal? Price,
    bool IsInStock,
    ProductStatus Status,
    DateTimeOffset Created,
    IReadOnlyCollection<ProductImageDto> Images,
    IReadOnlyCollection<ProductAttributeValueDto> AttributeValues);

public sealed record ProductImageDto(
    int Id,
    Guid ImageId,
    string? ImageUrl,
    int Order,
    bool IsPrimary);

public sealed record ProductAttributeValueDto(
    int Id,
    int AttributeId,
    string AttributeName,
    AttributeType Type,
    string Value,
    int? OptionId,
    string? OptionValue);
