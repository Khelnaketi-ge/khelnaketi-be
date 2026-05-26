using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;

public sealed class CreateAttributeOptionCommandHandlerValidation : AbstractValidator<CreateAttributeOptionCommand>
{
    public CreateAttributeOptionCommandHandlerValidation()
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Option value is required")
            .MaximumLength(160).WithMessage("Option value is too long");
    }
}
