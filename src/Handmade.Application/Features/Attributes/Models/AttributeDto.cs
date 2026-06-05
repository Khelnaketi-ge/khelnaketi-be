using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Attributes.Models;

public sealed record AttributeDto(
    int Id,
    string Name,
    AttributeType Type,
    string? Unit,
    bool IsDisabled,
    IReadOnlyCollection<AttributeTranslationDto> Translations,
    IReadOnlyCollection<AttributeOptionDto> Options);

public sealed record AttributeTranslationDto(
    string LanguageCode,
    string Name,
    string Slug);

public sealed record AttributeOptionDto(
    int Id,
    string Value,
    int Order,
    IReadOnlyCollection<AttributeOptionTranslationDto> Translations);

public sealed record AttributeOptionTranslationDto(
    string LanguageCode,
    string Value,
    string Slug);
