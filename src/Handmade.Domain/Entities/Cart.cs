using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class Cart : BaseAuditableEntity<int>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public CartStatus Status { get; set; } = CartStatus.Active;

    public ICollection<CartItem> Items { get; set; } = [];
}
