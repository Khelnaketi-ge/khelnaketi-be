using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class Category : BaseAuditableEntity<int>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }

    public int? ParentId { get; set; }
    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; } = [];
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
