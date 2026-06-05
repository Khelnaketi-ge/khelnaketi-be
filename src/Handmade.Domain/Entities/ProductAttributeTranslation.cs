using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class ProductAttributeTranslation : BaseAuditableEntity<int>
{
    public int ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public required string LanguageCode { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}
