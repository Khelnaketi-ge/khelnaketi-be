using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class BrandRole : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public required string Name { get; set; }
    public required string NormalizedName { get; set; }

    public bool IsSystemRole { get; set; }

    public Permissions Permissions { get; set; } = Permissions.None;

    public ICollection<BrandMember> Members { get; set; } = [];
    public ICollection<BrandInvitation> Invitations { get; set; } = [];
}
