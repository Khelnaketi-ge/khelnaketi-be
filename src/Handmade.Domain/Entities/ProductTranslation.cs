using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class ProductTranslation : BaseAuditableEntity<int>
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public required string LanguageCode { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
}
