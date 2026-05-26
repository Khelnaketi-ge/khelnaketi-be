using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;

public sealed record CreateAttributeOptionCommand(
    int AttributeId,
    string Value,
    int Order) : IRequest<AttributeOptionDto>;

public sealed class CreateAttributeOptionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateAttributeOptionCommand, AttributeOptionDto>
{
    public async Task<AttributeOptionDto> Handle(
        CreateAttributeOptionCommand request,
        CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        if (attribute.Type != AttributeType.Select)
        {
            throw new ValidationException(nameof(request.AttributeId), "Options can only be added to select attributes");
        }

        var normalizedValue = TextNormalizer.Normalize(request.Value);
        var duplicatedValue = await context.AttributeOptions.AnyAsync(
            x => x.ProductAttributeId == attribute.Id && x.NormalizedValue == normalizedValue,
            cancellationToken);

        if (duplicatedValue)
        {
            throw new ValidationException(nameof(request.Value), "Option value already exists for this attribute");
        }

        var option = new AttributeOption
        {
            ProductAttributeId = attribute.Id,
            Value = request.Value.Trim(),
            Order = request.Order
        };

        context.AttributeOptions.Add(option);
        await context.SaveChangesAsync(cancellationToken);

        return new AttributeOptionDto(option.Id, option.Value, option.Order);
    }
}
