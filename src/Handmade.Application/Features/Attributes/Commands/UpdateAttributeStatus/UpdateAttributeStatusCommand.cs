using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeStatus;

public sealed record UpdateAttributeStatusCommand(int AttributeId, bool IsDisabled) : IRequest<AttributeDto>;

public sealed class UpdateAttributeStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAttributeStatusCommand, AttributeDto>
{
    public async Task<AttributeDto> Handle(
        UpdateAttributeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        attribute.IsDisabled = request.IsDisabled;

        await context.SaveChangesAsync(cancellationToken);

        return AttributeMappings.ToDto(attribute);
    }
}
