using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttribute;

public sealed class UpdateAttributeCommandHandlerValidation : AbstractValidator<UpdateAttributeCommand>
{
    public UpdateAttributeCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");

        TranslationValidation.ValidateAttributeTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var ka = TranslationValidation.Georgian(
                    command.Translations,
                    translation => translation.LanguageCode);
                return !await context.ProductAttributes.AnyAsync(
                    x => x.Id != command.AttributeId
                        && x.Translations.Any(t =>
                            t.LanguageCode == LanguageCodes.Georgian
                            && t.Name == ka.Name.Trim()),
                    cancellationToken);
            })
            .WithMessage("Attribute name already exists");

        RuleFor(x => x.Unit)
            .MaximumLength(32).WithMessage("Attribute unit is too long");
    }
}
