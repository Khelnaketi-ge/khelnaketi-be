using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    int CategoryId,
    string Name,
    string? Description) : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(x => x.Children)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        category.Name = request.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentId,
            category.Children.Count == 0,
            [],
            []);
    }
}
