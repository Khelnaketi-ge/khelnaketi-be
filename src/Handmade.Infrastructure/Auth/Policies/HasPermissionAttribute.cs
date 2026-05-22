using Handmade.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Handmade.Infrastructure.Auth.Policies;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute(
    Permissions permission = default,
    bool isSuperAdminRequired = false,
    bool phoneVerifiedRequired = false)
    : AuthorizeAttribute(
        policy: 
        $"{PermissionAuthorizationPolicyProvider.PolicyPrefix}" +
        $"{permission}" +
        $"#{isSuperAdminRequired}" +
        $"#{phoneVerifiedRequired}"
    )
{}
