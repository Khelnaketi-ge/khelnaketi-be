using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class CategoryTranslation : BaseAuditableEntity<int>
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public required string LanguageCode { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}
