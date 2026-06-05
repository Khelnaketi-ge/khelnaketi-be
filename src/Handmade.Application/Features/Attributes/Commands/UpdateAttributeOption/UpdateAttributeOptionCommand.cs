using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;

public sealed record UpdateAttributeOptionCommand(
    int OptionId,
    int Order,
    IReadOnlyCollection<AttributeOptionTranslationInput> Translations) : IRequest<AttributeOptionDto>;

public sealed class UpdateAttributeOptionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAttributeOptionCommand, AttributeOptionDto>
{
    public async Task<AttributeOptionDto> Handle(
        UpdateAttributeOptionCommand request,
        CancellationToken cancellationToken)
    {
        var option = await context.AttributeOptions
            .Include(x => x.ProductAttribute)
            .Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == request.OptionId, cancellationToken);

        if (option is null)
        {
            throw new ValidationException(nameof(request.OptionId), "Option was not found");
        }

        if (option.ProductAttribute.Type != AttributeType.Select)
        {
            throw new ValidationException(nameof(request.OptionId), "Only select attribute options can be edited");
        }

        var kaTranslation = TranslationValidation.Georgian(
            request.Translations,
            translation => translation.LanguageCode);
        var duplicatedValue = await context.AttributeOptions.AnyAsync(
            x => x.Id != option.Id
                 && x.ProductAttributeId == option.ProductAttributeId
                 && x.Translations.Any(t =>
                     t.LanguageCode == LanguageCodes.Georgian
                     && t.Value == kaTranslation.Value.Trim()),
            cancellationToken);

        if (duplicatedValue)
        {
            throw new ValidationException(nameof(request.Translations), "Option value already exists for this attribute");
        }

        option.Order = request.Order;

        foreach (var translation in option.Translations.ToList())
        {
            context.AttributeOptionTranslations.Remove(translation);
        }

        option.Translations.Clear();
        foreach (var input in request.Translations)
        {
            option.Translations.Add(new AttributeOptionTranslation
            {
                AttributeOptionId = option.Id,
                LanguageCode = LanguageCodes.Normalize(input.LanguageCode),
                Value = input.Value.Trim(),
                Slug = SlugGenerator.GenerateForEntity(input.Value, "ao", option.Id, 200)
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new AttributeOptionDto(
            option.Id,
            AttributeMappings.GetOptionValue(option),
            option.Order,
            option.Translations
                .OrderBy(x => x.LanguageCode)
                .Select(x => new AttributeOptionTranslationDto(x.LanguageCode, x.Value, x.Slug))
                .ToList());
    }
}
