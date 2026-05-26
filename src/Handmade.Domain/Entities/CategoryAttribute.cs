using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class CategoryAttribute : BaseAuditableEntity<int>
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public int Order { get; set; }
}
