using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.UnlinkCategoryAttribute;

public sealed class UnlinkCategoryAttributeCommandHandlerValidation
    : AbstractValidator<UnlinkCategoryAttributeCommand>
{
    public UnlinkCategoryAttributeCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryAttributeId)
            .GreaterThan(0).WithMessage("Category attribute is required");
    }
}
