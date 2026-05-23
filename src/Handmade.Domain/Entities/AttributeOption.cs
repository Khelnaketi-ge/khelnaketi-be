using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class AttributeOption : BaseAuditableEntity<int>
{
    public int CategoryAttributeId { get; set; }
    public CategoryAttribute CategoryAttribute { get; set; } = null!;

    public required string Value { get; set; }
    public required string NormalizedValue { get; set; }
    public int Order { get; set; }
}
