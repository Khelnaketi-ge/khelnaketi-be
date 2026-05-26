using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;

namespace Handmade.Application.Features.Products.Models;

internal static class ProductMappings
{
    public static ProductDto ToDto(Product product, IImageStorageService imageStorage) =>
        new(
            product.Id,
            product.BrandId,
            product.CategoryId,
            product.Category.Name,
            product.Name,
            product.Description,
            product.Sku,
            product.Price,
            product.IsInStock,
            product.Status,
            product.Created,
            product.Images
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new ProductImageDto(
                    x.Id,
                    x.ImageId,
                    x.Image is null ? null : imageStorage.GetPublicUrl(x.Image.ObjectKey),
                    x.Order,
                    x.IsPrimary))
                .ToList(),
            product.AttributeValues
                .OrderBy(x => x.ProductAttribute.Name)
                .Select(x => new ProductAttributeValueDto(
                    x.Id,
                    x.ProductAttributeId,
                    x.ProductAttribute.Name,
                    x.ProductAttribute.Type,
                    x.Value,
                    x.AttributeOptionId,
                    x.AttributeOption?.Value))
                .ToList());
}
