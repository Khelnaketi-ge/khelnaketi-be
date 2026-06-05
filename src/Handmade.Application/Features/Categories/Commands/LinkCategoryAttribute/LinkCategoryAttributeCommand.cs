using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.LinkCategoryAttribute;

public sealed record LinkCategoryAttributeCommand(
    int CategoryId,
    int AttributeId,
    bool IsRequired,
    bool IsFilterable,
    int Order) : IRequest<CategoryAttributeDto>;

public sealed class LinkCategoryAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<LinkCategoryAttributeCommand, CategoryAttributeDto>
{
    public async Task<CategoryAttributeDto> Handle(
        LinkCategoryAttributeCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(x => x.Children)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        if (category.Children.Count > 0)
        {
            throw new ValidationException(nameof(request.CategoryId), "Attributes can only be linked to leaf categories");
        }

        var attribute = await context.ProductAttributes
            .Include(x => x.Options)
                .ThenInclude(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        if (attribute.IsDisabled)
        {
            throw new ValidationException(nameof(request.AttributeId), "Disabled attributes cannot be linked");
        }

        var alreadyLinked = await context.CategoryAttributes.AnyAsync(
            x => x.CategoryId == request.CategoryId && x.ProductAttributeId == request.AttributeId,
            cancellationToken);

        if (alreadyLinked)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute is already linked to this category");
        }

        var categoryAttribute = new CategoryAttribute
        {
            CategoryId = request.CategoryId,
            ProductAttributeId = request.AttributeId,
            ProductAttribute = attribute,
            IsRequired = request.IsRequired,
            IsFilterable = request.IsFilterable,
            Order = request.Order
        };

        context.CategoryAttributes.Add(categoryAttribute);
        await context.SaveChangesAsync(cancellationToken);

        return CategoryAttributeMappings.ToDto(categoryAttribute);
    }
}
