using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;

public sealed record UpdateAttributeOptionCommand(
    int OptionId,
    string Value,
    int Order) : IRequest<AttributeOptionDto>;

public sealed class UpdateAttributeOptionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAttributeOptionCommand, AttributeOptionDto>
{
    public async Task<AttributeOptionDto> Handle(
        UpdateAttributeOptionCommand request,
        CancellationToken cancellationToken)
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
            throw new ValidationException(nameof(request.OptionId), "Only select attribute options can be edited");
        }

        var normalizedValue = TextNormalizer.Normalize(request.Value);
        var duplicatedValue = await context.AttributeOptions.AnyAsync(
            x => x.Id != option.Id
                 && x.ProductAttributeId == option.ProductAttributeId
                 && x.NormalizedValue == normalizedValue,
            cancellationToken);

        if (duplicatedValue)
        {
            throw new ValidationException(nameof(request.Value), "Option value already exists for this attribute");
        }

        option.Value = request.Value.Trim();
        option.Order = request.Order;

        await context.SaveChangesAsync(cancellationToken);

        return new AttributeOptionDto(option.Id, option.Value, option.Order);
    }
}
