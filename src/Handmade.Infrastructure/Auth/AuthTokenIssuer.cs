using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Infrastructure.Auth;

public sealed class AuthTokenIssuer(
    IApplicationDbContext context,
    ITokenHasher tokenHasher,
    ITokenService tokenService,
    ICurrentRequest currentRequest,
    TimeProvider timeProvider) : IAuthTokenIssuer
{
    public async Task<TokensModel> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IpAddress = Truncate(currentRequest.IpAddress, 45),
            UserAgent = Truncate(currentRequest.UserAgent, 512),
            LastUsedAt = timeProvider.GetUtcNow()
        };

        var (refreshToken, refreshTokenExpiresAt) = tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            SessionId = session.Id,
            Session = session,
            TokenHash = tokenHasher.HashToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt
        };

        context.UserSessions.Add(session);
        context.RefreshTokens.Add(refreshTokenEntity);

        await context.SaveChangesAsync(cancellationToken);

        var ownsBrand = await context.Brands.AnyAsync(x => x.OwnerUserId == user.Id, cancellationToken);
        var (accessToken, accessTokenExpiresAt) = tokenService.CreateJwtToken(user, session.Id, ownsBrand);

        return new TokensModel(
            accessToken,
            accessTokenExpiresAt,
            refreshToken,
            refreshTokenExpiresAt);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
