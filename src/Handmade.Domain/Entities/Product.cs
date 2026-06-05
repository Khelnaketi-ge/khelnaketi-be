using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class Product : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? Sku { get; set; }

    public decimal? Price { get; set; }
    public bool IsInStock { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<ProductTranslation> Translations { get; set; } = [];
}
