using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class AttributeOption : BaseAuditableEntity<int>, INormalizedValueEntity
{
    public int CategoryAttributeId { get; set; }
    public CategoryAttribute CategoryAttribute { get; set; } = null!;

    public required string Value { get; set; }
    public string NormalizedValue { get; set; } = string.Empty;
    public int Order { get; set; }
}
