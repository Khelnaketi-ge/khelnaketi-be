using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.DeleteAttribute;

public sealed class DeleteAttributeCommandHandlerValidation : AbstractValidator<DeleteAttributeCommand>
{
    public DeleteAttributeCommandHandlerValidation()
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");
    }
}
