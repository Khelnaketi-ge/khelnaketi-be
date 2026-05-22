using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Handmade.Infrastructure.Auth.Services;

public sealed class GoogleAuthService(
    IApplicationDbContext context,
    IAuthTokenIssuer authTokenIssuer,
    ILogger<GoogleAuthService> logger,
    TimeProvider timeProvider) : IGoogleAuthService
{
    public async Task<TokensModel> ExternalLoginAsync(ExternalLoginModel login, CancellationToken cancellationToken)
    {
        if (login.Provider != Provider.Google)
        {
            logger.LogWarning("External login rejected: unsupported provider {Provider}", login.Provider);
            throw new UnauthorizedException(UnauthorizedErrors.InvalidExternalLogin);
        }

        var providerUserId = login.ProviderUserId.Trim();
        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            logger.LogWarning("External login rejected: missing provider user id for {Provider}", login.Provider);
            throw new UnauthorizedException(UnauthorizedErrors.InvalidExternalLogin);
        }

        try
        {
            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            var now = timeProvider.GetUtcNow();
            var providerEmail = login.Email?.Trim();
            var normalizedProviderEmail = string.IsNullOrWhiteSpace(providerEmail)
                ? null
                : NormalizeEmail(providerEmail);
            var displayName = Truncate(login.DisplayName?.Trim(), 200);

            var externalLogin = await context.UserExternalLogins
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Provider == Provider.Google && x.ProviderUserId == providerUserId,
                    cancellationToken);

            if (externalLogin is not null)
            {
                var tokens = await HandleExistingExternalLoginAsync(
                    externalLogin,
                    providerEmail,
                    displayName,
                    now,
                    cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return tokens;
            }

            if (normalizedProviderEmail is null || !login.EmailVerified)
            {
                logger.LogWarning("External login rejected: missing or unverified Google email");
                throw new UnauthorizedException(UnauthorizedErrors.InvalidExternalLogin);
            }

            var user = await context.Users.SingleOrDefaultAsync(
                x => x.NormalizedEmail == normalizedProviderEmail,
                cancellationToken);

            if (user is not null && user.IsBlocked)
            {
                logger.LogWarning(
                    "External login rejected: email-matched user {UserId} is blocked for Google",
                    user.Id);
                throw new UnauthorizedException(UnauthorizedErrors.UserBlocked);
            }

            var createdUser = user is null;
            user ??= CreateUserFromGoogle(providerEmail!, normalizedProviderEmail, displayName);

            if (createdUser)
            {
                context.Users.Add(user);
            }
            else
            {
                user.EmailVerified = true;
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
            }

            context.UserExternalLogins.Add(new UserExternalLogin
            {
                User = user,
                Provider = Provider.Google,
                ProviderUserId = providerUserId,
                ProviderEmail = Truncate(providerEmail, 320),
                ProviderDisplayName = displayName,
                LastUsedAt = now
            });

            await context.SaveChangesAsync(cancellationToken);
            var newLinkTokens = await authTokenIssuer.IssueTokensAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                createdUser
                    ? "External login succeeded: created user {UserId} via Google"
                    : "External login succeeded: linked existing user {UserId} via Google",
                user.Id);

            return newLinkTokens;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            logger.LogWarning(exception, "External login rejected: duplicate Google provider or email link race");
            throw new UnauthorizedException(UnauthorizedErrors.InvalidExternalLogin);
        }
    }

    private async Task<TokensModel> HandleExistingExternalLoginAsync(
        UserExternalLogin externalLogin,
        string? providerEmail,
        string? displayName,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (externalLogin.User.IsBlocked)
        {
            logger.LogWarning(
                "External login rejected: linked user {UserId} is blocked for Google",
                externalLogin.UserId);
            throw new UnauthorizedException(UnauthorizedErrors.UserBlocked);
        }

        externalLogin.ProviderEmail = Truncate(providerEmail, 320);
        externalLogin.ProviderDisplayName = displayName;
        externalLogin.LastUsedAt = now;
        externalLogin.User.AccessFailedCount = 0;
        externalLogin.User.LockoutEnd = null;

        await context.SaveChangesAsync(cancellationToken);
        var tokens = await authTokenIssuer.IssueTokensAsync(externalLogin.User, cancellationToken);

        logger.LogInformation(
            "External login succeeded: existing Google provider link for user {UserId}",
            externalLogin.UserId);

        return tokens;
    }

    private static User CreateUserFromGoogle(string email, string normalizedEmail, string? displayName)
    {
        var (firstName, lastName) = SplitDisplayName(displayName);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            EmailVerified = true,
            PhoneNumberVerified = false
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static (string firstName, string lastName) SplitDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (string.Empty, string.Empty);
        }

        var parts = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => (string.Empty, string.Empty),
            1 => (Truncate(parts[0], 100) ?? string.Empty, string.Empty),
            _ => (Truncate(parts[0], 100) ?? string.Empty, Truncate(parts[1], 100) ?? string.Empty)
        };
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
