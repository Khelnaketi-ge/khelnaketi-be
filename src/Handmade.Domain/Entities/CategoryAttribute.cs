using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class CategoryAttribute : BaseAuditableEntity<int>, INormalizedNameEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public required string Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    
    public AttributeType Type { get; set; }
    
    public string? Unit { get; set; }
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public int Order { get; set; }  

    public ICollection<AttributeOption> Options { get; set; } = [];
}
