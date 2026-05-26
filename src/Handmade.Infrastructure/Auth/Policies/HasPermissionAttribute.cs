using Microsoft.AspNetCore.Authorization;

namespace Handmade.Infrastructure.Auth.Policies;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute(
    bool isSuperAdminRequired = false,
    bool phoneVerifiedRequired = false,
    bool brandOwnerRequired = false)
    : AuthorizeAttribute(
        policy: 
        $"{PermissionAuthorizationPolicyProvider.PolicyPrefix}" +
        $"{isSuperAdminRequired}" +
        $"#{phoneVerifiedRequired}" +
        $"#{brandOwnerRequired}"
    )
{}
