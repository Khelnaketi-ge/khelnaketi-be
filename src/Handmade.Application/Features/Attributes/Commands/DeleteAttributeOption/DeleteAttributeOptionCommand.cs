using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.DeleteAttributeOption;

public sealed record DeleteAttributeOptionCommand(int OptionId) : IRequest;

public sealed class DeleteAttributeOptionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAttributeOptionCommand>
{
    public async Task Handle(DeleteAttributeOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await context.AttributeOptions
            .Include(x => x.ProductAttribute)
            .SingleOrDefaultAsync(x => x.Id == request.OptionId, cancellationToken);

        if (option is null)
        {
            throw new ValidationException(nameof(request.OptionId), "Option was not found");
        }

        if (option.ProductAttribute.Type != AttributeType.Select)
        {
            throw new ValidationException(nameof(request.OptionId), "Only select attribute options can be deleted");
        }

        context.AttributeOptions.Remove(option);
        await context.SaveChangesAsync(cancellationToken);
    }
}
