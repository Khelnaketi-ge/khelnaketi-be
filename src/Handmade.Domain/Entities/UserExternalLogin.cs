using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;
    
public class UserExternalLogin : BaseAuditableEntity<long>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Provider Provider { get; set; }
    public required string ProviderUserId { get; set; }
    
    public string? ProviderEmail { get; set; }
    public string? ProviderDisplayName { get; set; }
    
    public DateTimeOffset? LastUsedAt { get; set; }
}
