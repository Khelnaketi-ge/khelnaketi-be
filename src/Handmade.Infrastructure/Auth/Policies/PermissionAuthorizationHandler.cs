using Microsoft.AspNetCore.Authorization;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Infrastructure.Auth.Policies;

public class PermissionAuthorizationHandler(IApplicationDbContext dbContext) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authorizationContext,
        PermissionRequirement requirement)
    {
        var accessLevel = GetAccessLevel(authorizationContext);
        var isSuperAdmin = accessLevel == AccessLevel.SuperAdmin;

        if (requirement.SuperAdminRequired && !isSuperAdmin)
        {
            authorizationContext.Fail(new AuthorizationFailureReason(this, "SaRequired#User must be super admin"));
            return;
        }

        if (requirement.PhoneVerifiedRequired && !IsPhoneVerified(authorizationContext))
        {
            authorizationContext.Fail(new AuthorizationFailureReason(this, "PvRequired#User must verify phone number"));
            return;
        }

        if (requirement.BrandOwnerRequired && !isSuperAdmin && !await IsBrandOwnerAsync(authorizationContext))
        {
            authorizationContext.Fail(new AuthorizationFailureReason(this, "BoRequired#User must own a brand"));
            return;
        }

        authorizationContext.Succeed(requirement);
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
        return IsTrue(superAdminClaim?.Value) ? AccessLevel.SuperAdmin : AccessLevel.User;
    }

    private static bool IsPhoneVerified(AuthorizationHandlerContext context)
    {
        var phoneVerifiedClaim = context.User.Claims.FirstOrDefault(x => x.Type == Claims.PhoneVerified);
        return IsTrue(phoneVerifiedClaim?.Value);
    }

    private async Task<bool> IsBrandOwnerAsync(AuthorizationHandlerContext context)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == Claims.Id);

        return int.TryParse(userIdClaim?.Value, out var userId)
               && await dbContext.Brands.AnyAsync(x => x.OwnerUserId == userId);
    }

    private static bool IsTrue(string? value)
    {
        return value == "1" || (bool.TryParse(value, out var result) && result);
    }
}
