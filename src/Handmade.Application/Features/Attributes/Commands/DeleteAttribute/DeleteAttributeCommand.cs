using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.DeleteAttribute;

public sealed record DeleteAttributeCommand(int AttributeId) : IRequest;

public sealed class DeleteAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAttributeCommand>
{
    public async Task Handle(DeleteAttributeCommand request, CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .Include(x => x.Options)
            .Include(x => x.CategoryAttributes)
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        var attributeValues = await context.ProductAttributeValues
            .Where(x => x.ProductAttributeId == request.AttributeId)
            .ToListAsync(cancellationToken);

        context.ProductAttributeValues.RemoveRange(attributeValues);
        context.CategoryAttributes.RemoveRange(attribute.CategoryAttributes);
        context.AttributeOptions.RemoveRange(attribute.Options);
        context.ProductAttributes.Remove(attribute);

        await context.SaveChangesAsync(cancellationToken);
    }
}
