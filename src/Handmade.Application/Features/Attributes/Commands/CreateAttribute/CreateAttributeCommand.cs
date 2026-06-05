using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttribute;

public sealed record CreateAttributeCommand(
    AttributeType Type,
    string? Unit,
    IReadOnlyCollection<AttributeTranslationInput> Translations,
    IReadOnlyCollection<CreateAttributeOptionInput>? Options) : IRequest<AttributeDto>;

public sealed record CreateAttributeOptionInput(
    int Order,
    IReadOnlyCollection<AttributeOptionTranslationInput> Translations);

public sealed class CreateAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateAttributeCommand, AttributeDto>
{
    public async Task<AttributeDto> Handle(CreateAttributeCommand request, CancellationToken cancellationToken)
    {
        var optionInputs = request.Options ?? [];

        if (request.Type != AttributeType.Select && optionInputs.Count > 0)
        {
            throw new ValidationException(nameof(request.Options), "Options can only be added to select attributes");
        }

        var optionEntities = request.Type == AttributeType.Select
            ? optionInputs
                .Select(x => new AttributeOption
                {
                    Order = x.Order
                })
                .ToList()
            : [];

        var attribute = new ProductAttribute
        {
            Type = request.Type,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            Options = optionEntities
        };

        context.ProductAttributes.Add(attribute);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var input in request.Translations)
        {
            attribute.Translations.Add(new ProductAttributeTranslation
            {
                ProductAttributeId = attribute.Id,
                LanguageCode = LanguageCodes.Normalize(input.LanguageCode),
                Name = input.Name.Trim(),
                Slug = SlugGenerator.GenerateForEntity(input.Name, "a", attribute.Id, 200)
            });
        }

        for (var index = 0; index < optionEntities.Count; index++)
        {
            var option = optionEntities[index];
            var optionInput = optionInputs.ElementAt(index);

            foreach (var translation in optionInput.Translations)
            {
                option.Translations.Add(new AttributeOptionTranslation
                {
                    AttributeOptionId = option.Id,
                    LanguageCode = LanguageCodes.Normalize(translation.LanguageCode),
                    Value = translation.Value.Trim(),
                    Slug = SlugGenerator.GenerateForEntity(translation.Value, "ao", option.Id, 200)
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return AttributeMappings.ToDto(attribute);
    }
}
