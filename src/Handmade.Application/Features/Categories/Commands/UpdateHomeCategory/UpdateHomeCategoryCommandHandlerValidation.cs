using FluentValidation;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateHomeCategory;

public sealed class UpdateHomeCategoryCommandHandlerValidation
    : AbstractValidator<UpdateHomeCategoryCommand>
{
    public UpdateHomeCategoryCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required")
            .MustAsync(async (categoryId, cancellationToken) =>
                await context.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            .WithMessage("Category was not found");
    }
}
