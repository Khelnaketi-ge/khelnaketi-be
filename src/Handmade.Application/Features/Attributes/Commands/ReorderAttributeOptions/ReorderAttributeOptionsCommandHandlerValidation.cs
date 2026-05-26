using FluentValidation;

namespace Handmade.Application.Features.Attributes.Commands.ReorderAttributeOptions;

public sealed class ReorderAttributeOptionsCommandHandlerValidation : AbstractValidator<ReorderAttributeOptionsCommand>
{
    public ReorderAttributeOptionsCommandHandlerValidation()
    {
        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");

        RuleFor(x => x.Options)
            .NotNull().WithMessage("Options are required");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(x => x.OptionId)
                    .GreaterThan(0).WithMessage("Option is required");
            });
    }
}
