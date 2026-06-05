using Handmade.Domain.Entities;

namespace Handmade.Application.Features.Attributes.Models;

internal static class AttributeMappings
{
    public static AttributeDto ToDto(ProductAttribute attribute) =>
        new(
            attribute.Id,
            GetAttributeName(attribute),
            attribute.Type,
            attribute.Unit,
            attribute.IsDisabled,
            attribute.Translations
                .OrderBy(x => x.LanguageCode)
                .Select(x => new AttributeTranslationDto(x.LanguageCode, x.Name, x.Slug))
                .ToList(),
            attribute.Options
                .OrderBy(x => x.Order)
                .ThenBy(GetOptionValue)
                .Select(x => new AttributeOptionDto(
                    x.Id,
                    GetOptionValue(x),
                    x.Order,
                    x.Translations
                        .OrderBy(translation => translation.LanguageCode)
                        .Select(translation => new AttributeOptionTranslationDto(
                            translation.LanguageCode,
                            translation.Value,
                            translation.Slug))
                        .ToList()))
                .ToList());

    public static string GetAttributeName(ProductAttribute attribute) =>
        attribute.Translations
            .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)?.Name
        ?? attribute.Translations.FirstOrDefault()?.Name
        ?? $"Attribute {attribute.Id}";

    public static string GetOptionValue(AttributeOption option) =>
        option.Translations
            .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)?.Value
        ?? option.Translations.FirstOrDefault()?.Value
        ?? $"Option {option.Id}";
}
