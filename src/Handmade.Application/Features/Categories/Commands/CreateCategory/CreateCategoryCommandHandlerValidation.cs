using FluentValidation;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandlerValidation : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(160).WithMessage("Category name is too long")
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var normalizedName = TextNormalizer.Normalize(name);
                return !await context.Categories.AnyAsync(
                    x => x.ParentId == command.ParentId && x.NormalizedName == normalizedName,
                    cancellationToken);
            })
            .WithMessage("Category name already exists under this parent");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Category description is too long");
    }
}
