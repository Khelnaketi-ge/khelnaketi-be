using FluentValidation;

namespace Handmade.Application.Common.Localization;

public static class TranslationValidation
{
    public static void ValidateCategoryTranslations<T>(
        IRuleBuilderInitial<T, IReadOnlyCollection<CategoryTranslationInput>> rule)
    {
        rule.NotEmpty().WithMessage("Translations are required")
            .Must(HaveSupportedLanguageCodes).WithMessage("Translation language must be ka, en, or ru")
            .Must(HaveNoDuplicateLanguageCodes).WithMessage("Translation languages must be unique")
            .Must(HaveAllSupportedTranslations).WithMessage("Translations are required for ka, en, and ru")
            .Must(translations => HaveRequiredContent(translations, x => x.Name))
            .WithMessage("Provided translation names must not be empty");
    }

    public static void ValidateProductTranslations<T>(
        IRuleBuilderInitial<T, IReadOnlyCollection<ProductTranslationInput>> rule)
    {
        rule.NotEmpty().WithMessage("Translations are required")
            .Must(HaveSupportedLanguageCodes).WithMessage("Translation language must be ka, en, or ru")
            .Must(HaveNoDuplicateLanguageCodes).WithMessage("Translation languages must be unique")
            .Must(HaveAllSupportedTranslations).WithMessage("Translations are required for ka, en, and ru")
            .Must(translations => HaveRequiredContent(translations, x => x.Title))
            .WithMessage("Provided translation titles must not be empty");
    }

    public static void ValidateAttributeTranslations<T>(
        IRuleBuilderInitial<T, IReadOnlyCollection<AttributeTranslationInput>> rule)
    {
        rule.NotEmpty().WithMessage("Translations are required")
            .Must(HaveSupportedLanguageCodes).WithMessage("Translation language must be ka, en, or ru")
            .Must(HaveNoDuplicateLanguageCodes).WithMessage("Translation languages must be unique")
            .Must(HaveAllSupportedTranslations).WithMessage("Translations are required for ka, en, and ru")
            .Must(translations => HaveRequiredContent(translations, x => x.Name))
            .WithMessage("Provided translation names must not be empty");
    }

    public static void ValidateAttributeOptionTranslations<T>(
        IRuleBuilderInitial<T, IReadOnlyCollection<AttributeOptionTranslationInput>> rule)
    {
        rule.NotEmpty().WithMessage("Translations are required")
            .Must(HaveSupportedLanguageCodes).WithMessage("Translation language must be ka, en, or ru")
            .Must(HaveNoDuplicateLanguageCodes).WithMessage("Translation languages must be unique")
            .Must(HaveAllSupportedTranslations).WithMessage("Translations are required for ka, en, and ru")
            .Must(translations => HaveRequiredContent(translations, x => x.Value))
            .WithMessage("Provided translation values must not be empty");
    }

    public static T Georgian<T>(IEnumerable<T> translations, Func<T, string> languageSelector) =>
        translations.First(x => LanguageCodes.Normalize(languageSelector(x)) == LanguageCodes.Georgian);

    private static bool HaveSupportedLanguageCodes<T>(IEnumerable<T> translations)
        where T : notnull =>
        translations.All(x => LanguageCodes.IsSupported(GetLanguageCode(x)));

    private static bool HaveNoDuplicateLanguageCodes<T>(IEnumerable<T> translations)
        where T : notnull =>
        translations
            .Select(x => LanguageCodes.Normalize(GetLanguageCode(x)))
            .Distinct()
            .Count() == translations.Count();

    private static bool HaveAllSupportedTranslations<T>(IEnumerable<T> translations)
        where T : notnull =>
        LanguageCodes.Supported.All(languageCode =>
            translations.Any(x => LanguageCodes.Normalize(GetLanguageCode(x)) == languageCode));

    private static bool HaveRequiredContent<T>(IEnumerable<T> translations, Func<T, string> valueSelector) =>
        translations.All(x => string.IsNullOrWhiteSpace(GetLanguageCode(x)) || !string.IsNullOrWhiteSpace(valueSelector(x)));

    private static string GetLanguageCode<T>(T translation) =>
        translation switch
        {
            CategoryTranslationInput input => input.LanguageCode,
            ProductTranslationInput input => input.LanguageCode,
            AttributeTranslationInput input => input.LanguageCode,
            AttributeOptionTranslationInput input => input.LanguageCode,
            _ => string.Empty
        };
}
