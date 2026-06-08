using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;

public sealed class UpdateAttributeOptionCommandHandlerValidation : AbstractValidator<UpdateAttributeOptionCommand>
{
    public UpdateAttributeOptionCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.OptionId)
            .GreaterThan(0).WithMessage("Option is required");

        TranslationValidation.ValidateAttributeOptionTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (command, translations, cancellationToken) =>
            {
                var option = await context.AttributeOptions
                    .AsNoTracking()
                    .Where(x => x.Id == command.OptionId)
                    .Select(x => new { x.Id, x.ProductAttributeId })
                    .SingleOrDefaultAsync(cancellationToken);

                if (option is null)
                {
                    return true;
                }

                var ka = TranslationValidation.Georgian(
                    command.Translations,
                    translation => translation.LanguageCode);

                return !await context.AttributeOptions.AnyAsync(
                    x => x.Id != option.Id
                        && x.ProductAttributeId == option.ProductAttributeId
                        && x.Translations.Any(t =>
                            t.LanguageCode == LanguageCodes.Georgian
                            && t.Value == ka.Value.Trim()),
                    cancellationToken);
            })
            .WithMessage("Option value already exists for this attribute");
    }
}
