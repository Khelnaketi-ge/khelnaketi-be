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
            || !bool.TryParse(policies[0], out var isSuperAdminRequired)
            || !bool.TryParse(policies[1], out var phoneVerifiedRequired)
            || !bool.TryParse(policies[2], out var brandOwnerRequired))
        {
            throw new ApplicationException($"Invalid authorization policy name: {policyName}");
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(isSuperAdminRequired, phoneVerifiedRequired, brandOwnerRequired))
            .Build();
    }
}
