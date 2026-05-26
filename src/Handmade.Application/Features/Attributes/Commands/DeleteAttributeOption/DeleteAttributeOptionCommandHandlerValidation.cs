using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.DeleteAttributeOption;

public sealed class DeleteAttributeOptionCommandHandlerValidation : AbstractValidator<DeleteAttributeOptionCommand>
{
    public DeleteAttributeOptionCommandHandlerValidation()
    {
        RuleFor(x => x.OptionId)
            .GreaterThan(0).WithMessage("Option is required");
    }
}
