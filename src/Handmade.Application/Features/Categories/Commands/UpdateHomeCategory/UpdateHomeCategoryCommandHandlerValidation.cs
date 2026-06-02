using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.UpdateHomeCategory;

public sealed class UpdateHomeCategoryCommandHandlerValidation
    : AbstractValidator<UpdateHomeCategoryCommand>
{
    public UpdateHomeCategoryCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
    }
}
