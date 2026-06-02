using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandlerValidation : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
    }
}
