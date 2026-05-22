using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class VerificationCode : BaseAuditableEntity<long>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public VerificationCodePurpose Purpose { get; set; }

    public required string Destination { get; set; }
    public required string CodeHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public short FailedAttempts { get; set; }
}
