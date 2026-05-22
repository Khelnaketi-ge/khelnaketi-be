using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class BrandInvitation : BaseAuditableEntity<long>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }

    public int RoleId { get; set; }
    public BrandRole Role { get; set; } = null!;

    public int InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;

    public required string TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}