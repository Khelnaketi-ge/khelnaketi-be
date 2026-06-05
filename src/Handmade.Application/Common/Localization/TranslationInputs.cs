namespace Handmade.Application.Common.Localization;

public sealed record CategoryTranslationInput(
    string LanguageCode,
    string Name);

public sealed record ProductTranslationInput(
    string LanguageCode,
    string Title,
    string? ShortDescription,
    string? Description);

public sealed record AttributeTranslationInput(
    string LanguageCode,
    string Name);

public sealed record AttributeOptionTranslationInput(
    string LanguageCode,
    string Value);
