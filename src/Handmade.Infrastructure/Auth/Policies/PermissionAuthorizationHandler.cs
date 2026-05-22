using Microsoft.AspNetCore.Authorization;
using Handmade.Domain.Enums;

namespace Handmade.Infrastructure.Auth.Policies;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var accessLevel = GetAccessLevel(context);
        var isSuperAdmin = accessLevel == AccessLevel.SuperAdmin;

        if (requirement.SuperAdminRequired && !isSuperAdmin)
        {
            context.Fail(new AuthorizationFailureReason(this, "SaRequired#User must be super admin"));
            return Task.CompletedTask;
        }

        if (requirement.PhoneVerifiedRequired && !IsPhoneVerified(context))
        {
            context.Fail(new AuthorizationFailureReason(this, "PvRequired#User must verify phone number"));
            return Task.CompletedTask;
        }

        if (isSuperAdmin || requirement.Permission == Permissions.None || HasPermission(context, requirement.Permission))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }

    private static AccessLevel GetAccessLevel(AuthorizationHandlerContext context)
    {
        var accessLevelClaim = context.User.Claims.FirstOrDefault(x => x.Type == Claims.AccessLevel);

        if (short.TryParse(accessLevelClaim?.Value, out var numericAccessLevel)
            && Enum.IsDefined((AccessLevel)numericAccessLevel))
        {
            return (AccessLevel)numericAccessLevel;
        }

        if (Enum.TryParse(accessLevelClaim?.Value, ignoreCase: true, out AccessLevel namedAccessLevel)
            && Enum.IsDefined(namedAccessLevel))
        {
            return namedAccessLevel;
        }

        var superAdminClaim = context.User.Claims.FirstOrDefault(x => x.Type == Claims.SuperAdmin);
        if (IsTrue(superAdminClaim?.Value))
        {
            return AccessLevel.SuperAdmin;
        }

        return AccessLevel.User;
    }

    private static bool IsPhoneVerified(AuthorizationHandlerContext context)
    {
        var phoneVerifiedClaim = context.User.Claims.FirstOrDefault(x => x.Type == Claims.PhoneVerified);
        return IsTrue(phoneVerifiedClaim?.Value);
    }

    private static bool HasPermission(AuthorizationHandlerContext context, Permissions permission)
    {
        var permissionValue = ((int)permission).ToString();
        var permissionName = permission.ToString();

        return context.User.Claims
            .Where(x => x.Type == Claims.Permissions)
            .Any(x => x.Value == permissionValue || x.Value == permissionName);
    }

    private static bool IsTrue(string? value)
    {
        return value == "1" || (bool.TryParse(value, out var result) && result);
    }
}
