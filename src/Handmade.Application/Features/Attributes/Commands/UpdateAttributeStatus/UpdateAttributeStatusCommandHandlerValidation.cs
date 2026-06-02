using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeStatus;

public sealed class UpdateAttributeStatusCommandHandlerValidation
    : AbstractValidator<UpdateAttributeStatusCommand>
{
    public UpdateAttributeStatusCommandHandlerValidation()
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");
    }
}
