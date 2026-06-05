using Handmade.Application.Features.Attributes.Models;
using Handmade.Domain.Entities;

namespace Handmade.Application.Features.Categories.Queries.GetCategories;

internal static class CategoryAttributeMappings
{
    public static CategoryAttributeDto ToDto(CategoryAttribute attribute) =>
        new(
            attribute.Id,
            attribute.ProductAttributeId,
            AttributeMappings.GetAttributeName(attribute.ProductAttribute),
            attribute.ProductAttribute.Type,
            attribute.ProductAttribute.Unit,
            attribute.IsRequired,
            attribute.IsFilterable,
            attribute.Order,
            attribute.ProductAttribute.Options
                .OrderBy(option => option.Order)
                .ThenBy(AttributeMappings.GetOptionValue)
                .Select(option => new AttributeOptionDto(
                    option.Id,
                    AttributeMappings.GetOptionValue(option),
                    option.Order,
                    option.Translations
                        .OrderBy(translation => translation.LanguageCode)
                        .Select(translation => new AttributeOptionTranslationDto(
                            translation.LanguageCode,
                            translation.Value,
                            translation.Slug))
                        .ToList()))
                .ToList());
}
