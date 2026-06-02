using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(int CategoryId) : IRequest;

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(x => x.Children)
            .Include(x => x.CategoryAttributes)
            .Include(x => x.Products)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        if (category.Children.Count > 0)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category with children cannot be deleted");
        }

        if (category.Products.Count > 0)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category with products cannot be deleted");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
    }
}
