using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class ProductAttributeValue : BaseAuditableEntity<int>
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public int? AttributeOptionId { get; set; }
    public AttributeOption? AttributeOption { get; set; }

    public required string Value { get; set; }
}
