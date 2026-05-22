using Handmade.Domain.Common;
// ReSharper disable All

namespace Handmade.Domain.Entities;

public class RefreshToken : BaseAuditableEntity<long>
{
    public Guid SessionId { get; set; }
    public UserSession Session { get; set; } = null!;

    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    private bool IsRevoked => RevokedAt.HasValue;

    private bool IsExpired(DateTimeOffset now)
    {
        return now >= ExpiresAt;
    }

    public bool IsActive(DateTimeOffset now)
    {
        return !IsRevoked && !IsExpired(now);
    }
}
