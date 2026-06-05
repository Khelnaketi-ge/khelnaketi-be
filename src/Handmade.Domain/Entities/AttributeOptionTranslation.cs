using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class AttributeOptionTranslation : BaseAuditableEntity<int>
{
    public int AttributeOptionId { get; set; }
    public AttributeOption AttributeOption { get; set; } = null!;

    public required string LanguageCode { get; set; }
    public required string Value { get; set; }
    public required string Slug { get; set; }
}
