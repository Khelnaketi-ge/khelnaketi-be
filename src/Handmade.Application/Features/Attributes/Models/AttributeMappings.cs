using Handmade.Domain.Entities;

namespace Handmade.Application.Features.Attributes.Models;

internal static class AttributeMappings
{
    public static AttributeDto ToDto(ProductAttribute attribute) =>
        new(
            attribute.Id,
            attribute.Name,
            attribute.Type,
            attribute.Unit,
            attribute.Options
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Value)
                .Select(x => new AttributeOptionDto(x.Id, x.Value, x.Order))
                .ToList());
}
