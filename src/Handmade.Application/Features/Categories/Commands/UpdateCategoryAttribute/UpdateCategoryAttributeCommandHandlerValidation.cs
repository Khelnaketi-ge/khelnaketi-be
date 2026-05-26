using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategoryAttribute;

public sealed class UpdateCategoryAttributeCommandHandlerValidation : AbstractValidator<UpdateCategoryAttributeCommand>
{
    public UpdateCategoryAttributeCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryAttributeId)
            .GreaterThan(0).WithMessage("Category attribute is required");
    }
}
