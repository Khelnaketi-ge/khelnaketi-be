using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class UserSession : BaseAuditableEntity<Guid>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public DateTimeOffset? LastUsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    
    public bool IsRevoked => RevokedAt.HasValue;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}