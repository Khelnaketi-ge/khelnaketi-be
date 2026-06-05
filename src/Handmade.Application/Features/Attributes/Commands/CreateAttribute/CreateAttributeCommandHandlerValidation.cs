using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttribute;

public sealed class CreateAttributeCommandHandlerValidation : AbstractValidator<CreateAttributeCommand>
{
    public CreateAttributeCommandHandlerValidation(IApplicationDbContext context)
    {
        TranslationValidation.ValidateAttributeTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (name, cancellationToken) =>
            {
                var ka = TranslationValidation.Georgian(
                    name,
                    translation => translation.LanguageCode);
                return !await context.ProductAttributes.AnyAsync(
                    x => x.Translations.Any(t =>
                        t.LanguageCode == LanguageCodes.Georgian
                        && t.Name == ka.Name.Trim()),
                    cancellationToken);
            })
            .WithMessage("Attribute name already exists");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Attribute type is invalid");

        RuleFor(x => x.Unit)
            .MaximumLength(32).WithMessage("Attribute unit is too long");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                TranslationValidation.ValidateAttributeOptionTranslations(option.RuleFor(x => x.Translations));
            });

        When(x => x.Type == AttributeType.Select, () =>
        {
            RuleFor(x => x.Options)
                .Must(options => options is null || options
                    .Select(x => TranslationValidation.Georgian(
                        x.Translations,
                        translation => translation.LanguageCode).Value.Trim())
                    .Distinct()
                    .Count() == options.Count)
                .WithMessage("Option values must be unique");
        });
    }
}
