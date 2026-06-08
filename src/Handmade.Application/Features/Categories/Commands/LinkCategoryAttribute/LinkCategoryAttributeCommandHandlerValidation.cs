using FluentValidation;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.LinkCategoryAttribute;

public sealed class LinkCategoryAttributeCommandHandlerValidation : AbstractValidator<LinkCategoryAttributeCommand>
{
    public LinkCategoryAttributeCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required")
            .MustAsync(async (command, attributeId, cancellationToken) =>
                !await context.CategoryAttributes.AnyAsync(
                    x => x.CategoryId == command.CategoryId
                        && x.ProductAttributeId == attributeId,
                    cancellationToken))
            .WithMessage("Attribute is already linked to this category");
    }
}
