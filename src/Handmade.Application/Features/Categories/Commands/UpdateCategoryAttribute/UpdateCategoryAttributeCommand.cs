using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategoryAttribute;

public sealed record UpdateCategoryAttributeCommand(
    int CategoryAttributeId,
    bool IsRequired,
    bool IsFilterable,
    int Order) : IRequest<CategoryAttributeDto>;

public sealed class UpdateCategoryAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryAttributeCommand, CategoryAttributeDto>
{
    public async Task<CategoryAttributeDto> Handle(
        UpdateCategoryAttributeCommand request,
        CancellationToken cancellationToken)
    {
        var categoryAttribute = await context.CategoryAttributes
            .Include(x => x.ProductAttribute)
                .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryAttributeId, cancellationToken);

        if (categoryAttribute is null)
        {
            throw new ValidationException(nameof(request.CategoryAttributeId), "Category attribute was not found");
        }

        categoryAttribute.IsRequired = request.IsRequired;
        categoryAttribute.IsFilterable = request.IsFilterable;
        categoryAttribute.Order = request.Order;

        await context.SaveChangesAsync(cancellationToken);

        return CategoryAttributeMappings.ToDto(categoryAttribute);
    }
}
