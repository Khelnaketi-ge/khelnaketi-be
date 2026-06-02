using FluentValidation;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandlerValidation : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(160).WithMessage("Category name is too long")
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var category = await context.Categories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

                if (category is null)
                {
                    return true;
                }

                var normalizedName = TextNormalizer.Normalize(name);
                return !await context.Categories.AnyAsync(
                    x => x.Id != command.CategoryId
                        && x.ParentId == category.ParentId
                        && x.NormalizedName == normalizedName,
                    cancellationToken);
            })
            .WithMessage("Category name already exists under this parent");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Category description is too long");
    }
}
