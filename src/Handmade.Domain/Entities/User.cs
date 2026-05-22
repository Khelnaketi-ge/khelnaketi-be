using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class User : BaseAuditableEntity<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public bool EmailVerified { get; set; }
    
    public string? PhoneNumber { get; set; }
    public string? NormalizedPhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; }
    
    public string? PasswordHash { get; set; }
    public int TokenVersion { get; set; } = 1;
    public int PermissionVersion { get; set; } = 1;
    
    public short AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    
    public bool IsBlocked { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.User;
    
    public ICollection<UserSession> Sessions { get; set; } = [];
    public ICollection<VerificationCode> VerificationCodes { get; set; } = [];
}
