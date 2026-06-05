using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;

public sealed record CreateAttributeOptionCommand(
    int AttributeId,
    int Order,
    IReadOnlyCollection<AttributeOptionTranslationInput> Translations) : IRequest<AttributeOptionDto>;

public sealed class CreateAttributeOptionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateAttributeOptionCommand, AttributeOptionDto>
{
    public async Task<AttributeOptionDto> Handle(
        CreateAttributeOptionCommand request,
        CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        if (attribute.Type != AttributeType.Select)
        {
            throw new ValidationException(nameof(request.AttributeId), "Options can only be added to select attributes");
        }

        var kaTranslation = TranslationValidation.Georgian(
            request.Translations,
            translation => translation.LanguageCode);
        var duplicatedValue = await context.AttributeOptions.AnyAsync(
            x => x.ProductAttributeId == attribute.Id
                && x.Translations.Any(t =>
                    t.LanguageCode == LanguageCodes.Georgian
                    && t.Value == kaTranslation.Value.Trim()),
            cancellationToken);

        if (duplicatedValue)
        {
            throw new ValidationException(nameof(request.Translations), "Option value already exists for this attribute");
        }

        var option = new AttributeOption
        {
            ProductAttributeId = attribute.Id,
            Order = request.Order
        };

        context.AttributeOptions.Add(option);
        await context.SaveChangesAsync(cancellationToken);

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
