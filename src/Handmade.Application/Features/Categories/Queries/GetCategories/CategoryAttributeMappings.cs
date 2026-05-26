using Handmade.Application.Features.Attributes.Models;
using Handmade.Domain.Entities;

namespace Handmade.Application.Features.Categories.Queries.GetCategories;

internal static class CategoryAttributeMappings
{
    public static CategoryAttributeDto ToDto(CategoryAttribute attribute) =>
        new(
            attribute.Id,
            attribute.ProductAttributeId,
            attribute.ProductAttribute.Name,
            attribute.ProductAttribute.Type,
            attribute.ProductAttribute.Unit,
            attribute.IsRequired,
            attribute.IsFilterable,
            attribute.Order,
            attribute.ProductAttribute.Options
                .OrderBy(option => option.Order)
                .ThenBy(option => option.Value)
                .Select(option => new AttributeOptionDto(option.Id, option.Value, option.Order))
                .ToList());
}
