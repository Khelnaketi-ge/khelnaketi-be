using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.ReorderAttributeOptions;

public sealed record ReorderAttributeOptionsCommand(
    int AttributeId,
    IReadOnlyCollection<AttributeOptionOrderInput> Options) : IRequest<IReadOnlyCollection<AttributeOptionDto>>;

public sealed record AttributeOptionOrderInput(
    int OptionId,
    int Order);

public sealed class ReorderAttributeOptionsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReorderAttributeOptionsCommand, IReadOnlyCollection<AttributeOptionDto>>
{
    public async Task<IReadOnlyCollection<AttributeOptionDto>> Handle(
        ReorderAttributeOptionsCommand request,
        CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        if (attribute.Type != AttributeType.Select)
        {
            throw new ValidationException(nameof(request.AttributeId), "Only select attribute options can be reordered");
        }

        var requestedOptionIds = request.Options.Select(x => x.OptionId).ToList();

        if (requestedOptionIds.Count != requestedOptionIds.Distinct().Count())
        {
            throw new ValidationException(nameof(request.Options), "Option ids must be unique");
        }

        foreach (var optionOrder in request.Options)
        {
            var option = attribute.Options.SingleOrDefault(x => x.Id == optionOrder.OptionId);

            if (option is null)
            {
                throw new ValidationException(nameof(request.Options), "Option does not belong to this attribute");
            }

            option.Order = optionOrder.Order;
        }

        await context.SaveChangesAsync(cancellationToken);

        return attribute.Options
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Value)
            .Select(x => new AttributeOptionDto(x.Id, x.Value, x.Order))
            .ToList();
    }
}
