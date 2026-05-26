using Microsoft.AspNetCore.Authorization;

namespace Handmade.Infrastructure.Auth.Policies;

public class PermissionRequirement(
    bool superAdminRequired, 
    bool phoneVerifiedRequired,
    bool brandOwnerRequired) : IAuthorizationRequirement
{
    public bool SuperAdminRequired { get; } = superAdminRequired;
    public bool PhoneVerifiedRequired { get; } = phoneVerifiedRequired;
    public bool BrandOwnerRequired { get; } = brandOwnerRequired;
}
