using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class ProductAttribute : BaseAuditableEntity<int>
{
    public AttributeType Type { get; set; }
    public string? Unit { get; set; }
    public bool IsDisabled { get; set; }

    public ICollection<AttributeOption> Options { get; set; } = [];
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = [];
    public ICollection<ProductAttributeTranslation> Translations { get; set; } = [];
}
