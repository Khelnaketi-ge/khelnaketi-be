using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UnlinkCategoryAttribute;

public sealed record UnlinkCategoryAttributeCommand(int CategoryAttributeId) : IRequest;

public sealed class UnlinkCategoryAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnlinkCategoryAttributeCommand>
{
    public async Task Handle(UnlinkCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var categoryAttribute = await context.CategoryAttributes
            .SingleOrDefaultAsync(x => x.Id == request.CategoryAttributeId, cancellationToken);

        if (categoryAttribute is null)
        {
            throw new ValidationException(
                nameof(request.CategoryAttributeId),
                "Category attribute was not found");
        }

        context.CategoryAttributes.Remove(categoryAttribute);
        await context.SaveChangesAsync(cancellationToken);
    }
}
