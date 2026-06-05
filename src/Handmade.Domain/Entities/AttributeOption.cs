using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class AttributeOption : BaseAuditableEntity<int>
{
    public int ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public int Order { get; set; }
    public ICollection<AttributeOptionTranslation> Translations { get; set; } = [];
}
