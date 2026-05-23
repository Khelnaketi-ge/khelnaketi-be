using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class ProductImage : BaseAuditableEntity<int>
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid ImageId { get; set; }
    public ImageAsset Image { get; set; } = null!;

    public int Order { get; set; }
    public bool IsPrimary { get; set; }
}
