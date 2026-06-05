using FluentValidation;
using Handmade.Application.Common.Localization;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;

public sealed class UpdateAttributeOptionCommandHandlerValidation : AbstractValidator<UpdateAttributeOptionCommand>
{
    public UpdateAttributeOptionCommandHandlerValidation()
    {
        RuleFor(x => x.OptionId)
            .GreaterThan(0).WithMessage("Option is required");

        TranslationValidation.ValidateAttributeOptionTranslations(RuleFor(x => x.Translations));
    }
}
