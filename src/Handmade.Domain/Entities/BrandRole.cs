using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class BrandRole : BaseAuditableEntity<int>, INormalizedNameEntity
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public required string Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public Permissions Permissions { get; set; } = Permissions.None;

    public ICollection<BrandMember> Members { get; set; } = [];
    public ICollection<BrandInvitation> Invitations { get; set; } = [];
}
