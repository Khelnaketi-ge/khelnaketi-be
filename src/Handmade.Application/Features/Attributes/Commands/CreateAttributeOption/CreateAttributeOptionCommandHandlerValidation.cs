using FluentValidation;
using Handmade.Application.Common.Localization;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;

public sealed class CreateAttributeOptionCommandHandlerValidation : AbstractValidator<CreateAttributeOptionCommand>
{
    public CreateAttributeOptionCommandHandlerValidation()
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");

        TranslationValidation.ValidateAttributeOptionTranslations(RuleFor(x => x.Translations));
    }
}
