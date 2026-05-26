using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttribute;

public sealed record CreateAttributeCommand(
    string Name,
    AttributeType Type,
    string? Unit,
    IReadOnlyCollection<CreateAttributeOptionInput>? Options) : IRequest<AttributeDto>;

public sealed record CreateAttributeOptionInput(
    string Value,
    int Order);

public sealed class CreateAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateAttributeCommand, AttributeDto>
{
    public async Task<AttributeDto> Handle(CreateAttributeCommand request, CancellationToken cancellationToken)
    {
        var optionInputs = request.Options ?? [];

        if (request.Type != AttributeType.Select && optionInputs.Count > 0)
        {
            throw new ValidationException(nameof(request.Options), "Options can only be added to select attributes");
        }

        var attribute = new ProductAttribute
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            Options = request.Type == AttributeType.Select
                ? optionInputs
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => new AttributeOption
                    {
                        Value = x.Value.Trim(),
                        Order = x.Order
                    })
                    .ToList()
                : []
        };

        context.ProductAttributes.Add(attribute);
        await context.SaveChangesAsync(cancellationToken);

        return AttributeMappings.ToDto(attribute);
    }
}
