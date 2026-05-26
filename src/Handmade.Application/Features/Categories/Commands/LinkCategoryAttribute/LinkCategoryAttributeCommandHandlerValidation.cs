using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.LinkCategoryAttribute;

public sealed class LinkCategoryAttributeCommandHandlerValidation : AbstractValidator<LinkCategoryAttributeCommand>
{
    public LinkCategoryAttributeCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.AttributeId)
            .GreaterThan(0).WithMessage("Attribute is required");
    }
}
