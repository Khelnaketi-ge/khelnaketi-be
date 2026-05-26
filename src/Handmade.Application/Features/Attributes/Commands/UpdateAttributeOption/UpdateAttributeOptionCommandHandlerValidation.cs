using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;

public sealed class UpdateAttributeOptionCommandHandlerValidation : AbstractValidator<UpdateAttributeOptionCommand>
{
    public UpdateAttributeOptionCommandHandlerValidation()
    {
        RuleFor(x => x.OptionId)
            .GreaterThan(0).WithMessage("Option is required");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Option value is required")
            .MaximumLength(160).WithMessage("Option value is too long");
    }
}
