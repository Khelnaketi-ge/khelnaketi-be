using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;

public sealed class CreateAttributeOptionCommandHandlerValidation : AbstractValidator<CreateAttributeOptionCommand>
{
    public CreateAttributeOptionCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");

        TranslationValidation.ValidateAttributeOptionTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (command, translations, cancellationToken) =>
            {
                var ka = TranslationValidation.Georgian(
                    command.Translations,
                    translation => translation.LanguageCode);

                return !await context.AttributeOptions.AnyAsync(
                    x => x.ProductAttributeId == command.AttributeId
                        && x.Translations.Any(t =>
                            t.LanguageCode == LanguageCodes.Georgian
                            && t.Value == ka.Value.Trim()),
                    cancellationToken);
            })
            .WithMessage("Option value already exists for this attribute");
    }
}
