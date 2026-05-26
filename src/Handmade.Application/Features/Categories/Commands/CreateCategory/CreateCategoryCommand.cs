using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    int? ParentId) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await context.Categories
                .Include(x => x.CategoryAttributes)
                .SingleOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
            {
                throw new ValidationException(nameof(request.ParentId), "Parent category was not found");
            }

            if (parent.CategoryAttributes.Count > 0)
            {
                throw new ValidationException(
                    nameof(request.ParentId),
                    "Child categories cannot be added under a category with linked attributes");
            }
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ParentId = request.ParentId
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentId,
            true,
            [],
            []);
    }
}
