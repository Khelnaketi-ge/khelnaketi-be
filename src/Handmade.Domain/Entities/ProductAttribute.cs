using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class ProductAttribute : BaseAuditableEntity<int>, INormalizedNameEntity
{
    public required string Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;

    public AttributeType Type { get; set; }
    public string? Unit { get; set; }
    public bool IsDisabled { get; set; }

    public ICollection<AttributeOption> Options { get; set; } = [];
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = [];
}
