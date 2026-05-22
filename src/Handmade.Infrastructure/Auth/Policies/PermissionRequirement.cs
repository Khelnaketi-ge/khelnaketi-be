using Handmade.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Handmade.Infrastructure.Auth.Policies;

public class PermissionRequirement(
    Permissions permission, 
    bool superAdminRequired, 
    bool phoneVerifiedRequired) : IAuthorizationRequirement
{
    public Permissions Permission { get; } = permission;
    public bool SuperAdminRequired { get; } = superAdminRequired;
    public bool PhoneVerifiedRequired { get; } = phoneVerifiedRequired;
}