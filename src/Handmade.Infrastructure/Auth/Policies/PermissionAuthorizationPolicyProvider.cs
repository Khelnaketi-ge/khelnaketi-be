using Handmade.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ApplicationException = Handmade.Application.Common.Exceptions.ApplicationException;

namespace Handmade.Infrastructure.Auth.Policies;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "Permission:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        if (!policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var policies = policyName[PolicyPrefix.Length..].Split('#');

        if (policies.Length != 3
            || !Enum.TryParse(policies[0], ignoreCase: false, out Permissions permission)
            || !Enum.IsDefined(permission)
            || !bool.TryParse(policies[1], out var isSuperAdminRequired)
            || !bool.TryParse(policies[2], out var phoneVerifiedRequired))
        {
            throw new ApplicationException($"Invalid authorization policy name: {policyName}");
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission, isSuperAdminRequired, phoneVerifiedRequired))
            .Build();
    }
}
