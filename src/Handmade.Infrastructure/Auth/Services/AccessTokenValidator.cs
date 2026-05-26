using System.Security.Claims;
using EFCoreSecondLevelCacheInterceptor;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Infrastructure.Auth.Services;

public sealed class AccessTokenValidator(
    IApplicationDbContext context,
    TimeProvider timeProvider) : IAccessTokenValidator
{
    public async Task<AccessTokenValidationResult> ValidateAsync(
        ClaimsPrincipal? principal,
        CancellationToken cancellationToken)
    {
        if (!TryGetClaim<int>(principal, Claims.Id, int.TryParse, out var userId))
        {
            return AccessTokenValidationResult.Failure("Missing or invalid user id claim.");
        }

        if (!TryGetClaim<Guid>(principal, Claims.SessionId, Guid.TryParse, out var sessionId))
        {
            return AccessTokenValidationResult.Failure("Missing or invalid session id claim.");
        }

        if (!TryGetClaim<int>(principal, Claims.TokenVersion, int.TryParse, out var tokenVersion))
        {
            return AccessTokenValidationResult.Failure("Missing or invalid token version claim.");
        }

        var user = await context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id,
                x.IsBlocked,
                x.TokenVersion
            })
            .Cacheable(CacheExpirationMode.Absolute, TimeSpan.FromSeconds(30))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return AccessTokenValidationResult.Failure("User does not exist.");
        }

        if (user.IsBlocked)
        {
            return AccessTokenValidationResult.Failure("User is blocked.");
        }

        if (user.TokenVersion != tokenVersion)
        {
            return AccessTokenValidationResult.Failure("Token version is invalid.");
        }

        var session = await context.UserSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

        if (session is null)
        {
            return AccessTokenValidationResult.Failure("Session does not exist.");
        }

        if (session.RevokedAt.HasValue)
        {
            return AccessTokenValidationResult.Failure("Session is revoked.");
        }

        var now = timeProvider.GetUtcNow();

        if (session.LastUsedAt.HasValue && session.LastUsedAt.Value.AddMinutes(1) > now)
            return AccessTokenValidationResult.Success();
        
        session.LastUsedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        return AccessTokenValidationResult.Success();
    }

    private static bool TryGetClaim<T>(
        ClaimsPrincipal? principal,
        string claimType,
        TryParse<T> tryParse,
        out T result)
    {
        result = default!;

        var value = principal?.FindFirst(claimType)?.Value;
        return value is not null && tryParse(value, out result);
    }

    private delegate bool TryParse<T>(string value, out T result);
}
