using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class HomeCategory : BaseAuditableEntity<int>
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public Guid? ImageId { get; set; }
    public ImageAsset? Image { get; set; }
    public int Order { get; set; }
}
