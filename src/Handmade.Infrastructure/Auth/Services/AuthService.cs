using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Infrastructure.Auth.Services;

public class AuthService(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenHasher tokenHasher,
    ITokenService tokenService,
    IAuthTokenIssuer authTokenIssuer,
    IEmailSender emailSender,
    ICurrentRequest currentRequest,
    TimeProvider timeProvider) : IAuthService
{
    private const short MaxAttempts = 10;
    private const short MaxEmailCodeAttempts = 5;
    private const short MaxCodeSendAttemptsPerHour = 5;
    private static readonly TimeSpan EmailVerificationCodeTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PasswordResetCodeTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CodeSendCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan CodeSendLimitWindow = TimeSpan.FromHours(1);

    public async Task RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken)
    {
        if (password != confirmPassword)
        {
            throw new ValidationException("ConfirmPassword", "Password and confirm password do not match");
        }

        var normalizedEmail = NormalizeEmail(email);
        var emailAddress = email.Trim();

        var emailExists = await context.Users.AnyAsync(
            x => x.NormalizedEmail == normalizedEmail
                 || x.Email.Trim().ToUpper() == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            throw new ValidationException("Email", "Email address is already registered");
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = emailAddress,
            NormalizedEmail = normalizedEmail,
            EmailVerified = false,
            PhoneNumberVerified = false,
            PasswordHash = passwordHasher.HashPassword(password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        await SendEmailCodeAsync(user.Id, cancellationToken);
    }

    public async Task<TokensModel> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var normalizedEmail = NormalizeEmail(email);

        var user = await context.Users.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user?.PasswordHash is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        if (user.IsBlocked)
        {
            throw new UnauthorizedException(UnauthorizedErrors.UserBlocked);
        }

        if (!user.EmailVerified)
        {
            throw new UnauthorizedException(UnauthorizedErrors.EmailNotVerified);
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > now)
        {
            throw new UnauthorizedException(UnauthorizedErrors.UserLockedOut);
        }

        if (!passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            RegisterFailedLogin(user, now);
            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        return await authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }

    public async Task<TokensModel> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        if (!tokenService.ValidateToken(accessToken, out var jwtToken, validateLifetime: false))
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        var userId = GetRequiredClaim<int>(jwtToken, Claims.Id, int.TryParse);
        var sessionId = GetRequiredClaim<Guid>(jwtToken, Claims.SessionId, Guid.TryParse);
        var tokenVersion = GetRequiredClaim<int>(jwtToken, Claims.TokenVersion, int.TryParse);
        var permissionVersion = GetRequiredClaim<int>(jwtToken, Claims.PermissionVersion, int.TryParse);

        var now = timeProvider.GetUtcNow();
        var refreshTokenHash = tokenHasher.HashToken(refreshToken);

        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        var user = await context.Users
            .Include(x => x.Sessions.Where(session => session.Id == sessionId))
                .ThenInclude(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null
            || user.IsBlocked
            || !user.EmailVerified
            || user.TokenVersion != tokenVersion
            || user.PermissionVersion != permissionVersion)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        var session = user.Sessions.SingleOrDefault();
        if (session is null || session.RevokedAt.HasValue)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        var storedRefreshToken = session.RefreshTokens.SingleOrDefault(x => x.TokenHash == refreshTokenHash);
        if (storedRefreshToken is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        if (!storedRefreshToken.IsActive(now))
        {
            if (storedRefreshToken is { RevokedAt: not null, ReplacedByTokenHash: not null })
            {
                RevokeSession(session, now, "Refresh token reuse detected");
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        var (newRefreshToken, newRefreshTokenExpiresAt) = tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = tokenHasher.HashToken(newRefreshToken);

        var rotatedRows = await context.RefreshTokens
            .Where(x => x.Id == storedRefreshToken.Id && x.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.RevokedReason, "Refresh token rotated")
                    .SetProperty(x => x.ReplacedByTokenHash, newRefreshTokenHash),
                cancellationToken);

        if (rotatedRows != 1)
        {
            var latestTokenState = await context.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == storedRefreshToken.Id, cancellationToken);

            if (latestTokenState is { RevokedAt: not null, ReplacedByTokenHash: not null })
            {
                RevokeSession(session, now, "Refresh token reuse detected");
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        session.LastUsedAt = now;
        session.IpAddress = Truncate(currentRequest.IpAddress, 45);
        session.UserAgent = Truncate(currentRequest.UserAgent, 512);

        context.RefreshTokens.Add(new RefreshToken
        {
            SessionId = session.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = newRefreshTokenExpiresAt
        });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var (newAccessToken, newAccessTokenExpiresAt) = tokenService.CreateJwtToken(user, session.Id);

        return new TokensModel(
            newAccessToken,
            newAccessTokenExpiresAt,
            newRefreshToken,
            newRefreshTokenExpiresAt);
    }

    public async Task LogoutAsync(int userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var session = await context.UserSessions
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId && x.UserId == userId,
                cancellationToken);

        if (session is null)
        {
            return;
        }

        RevokeSession(session, now, "User logout");

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await context.Users.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user is null || user.IsBlocked || !user.EmailVerified)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        if (!await CanSendVerificationCodeAsync(
                user.Id,
                normalizedEmail,
                VerificationCodePurpose.PasswordReset,
                now,
                cancellationToken))
        {
            return;
        }

        var code = GenerateVerificationCode();

        await InvalidateUnusedVerificationCodesAsync(
            user.Id,
            normalizedEmail,
            VerificationCodePurpose.PasswordReset,
            now,
            cancellationToken);

        var verificationCode = new VerificationCode
        {
            UserId = user.Id,
            Purpose = VerificationCodePurpose.PasswordReset,
            Destination = user.NormalizedEmail,
            CodeHash = tokenHasher.HashToken(code),
            ExpiresAt = now.Add(PasswordResetCodeTtl)
        };

        context.VerificationCodes.Add(verificationCode);
        await context.SaveChangesAsync(cancellationToken);

        await SendPasswordResetCodeEmailAsync(user, code, cancellationToken);
    }

    public async Task ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        string confirmNewPassword,
        CancellationToken cancellationToken)
    {
        if (newPassword != confirmNewPassword)
        {
            throw new ValidationException("ConfirmNewPassword", "Password and confirm password do not match");
        }

        var normalizedEmail = NormalizeEmail(email);
        var now = timeProvider.GetUtcNow();

        var user = await context.Users
            .Include(x => x.Sessions)
                .ThenInclude(x => x.RefreshTokens)
            .SingleOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is null || user.IsBlocked || !user.EmailVerified)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidPasswordResetCode);
        }

        var verificationCode = await context.VerificationCodes
            .Where(x =>
                x.UserId == user.Id
                && x.Purpose == VerificationCodePurpose.PasswordReset
                && x.Destination == normalizedEmail
                && x.UsedAt == null)
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(cancellationToken);

        if (verificationCode is null
            || verificationCode.ExpiresAt <= now
            || verificationCode.FailedAttempts >= MaxEmailCodeAttempts)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidPasswordResetCode);
        }

        if (!tokenHasher.VerifyToken(code, verificationCode.CodeHash))
        {
            verificationCode.FailedAttempts++;
            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(UnauthorizedErrors.InvalidPasswordResetCode);
        }

        user.PasswordHash = passwordHasher.HashPassword(newPassword);
        user.TokenVersion++;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        verificationCode.UsedAt = now;

        foreach (var session in user.Sessions.Where(x => !x.RevokedAt.HasValue))
        {
            RevokeSession(session, now, "Password reset");
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResendVerificationCodeAsync(
        string email,
        VerificationCodePurpose purpose,
        CancellationToken cancellationToken)
    {
        switch (purpose)
        {
            case VerificationCodePurpose.EmailVerification:
                await ResendEmailVerificationCodeAsync(email, cancellationToken);
                return;
            case VerificationCodePurpose.PasswordReset:
                await RequestPasswordResetAsync(email, cancellationToken);
                return;
            default:
                throw new UnauthorizedException(UnauthorizedErrors.InvalidEmailVerificationCode);
        }
    }

    public Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken)
    {
        throw new Handmade.Application.Common.Exceptions.ApplicationException("Change password is not implemented.");
    }

    public async Task SendEmailCodeAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidEmailVerificationCode);
        }

        if (user.EmailVerified)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        if (!await CanSendVerificationCodeAsync(
                user.Id,
                user.NormalizedEmail,
                VerificationCodePurpose.EmailVerification,
                now,
                cancellationToken))
        {
            throw new DomainException(UnauthorizedErrors.TooManyVerificationCodeRequests);
        }

        var code = GenerateVerificationCode();

        await InvalidateUnusedVerificationCodesAsync(
            user.Id,
            user.NormalizedEmail,
            VerificationCodePurpose.EmailVerification,
            now,
            cancellationToken);

        var verificationCode = new VerificationCode
        {
            UserId = user.Id,
            Purpose = VerificationCodePurpose.EmailVerification,
            Destination = user.NormalizedEmail,
            CodeHash = tokenHasher.HashToken(code),
            ExpiresAt = now.Add(EmailVerificationCodeTtl)
        };

        context.VerificationCodes.Add(verificationCode);
        await context.SaveChangesAsync(cancellationToken);

        await SendEmailVerificationCodeEmailAsync(user, code, cancellationToken);
    }

    private async Task ResendEmailVerificationCodeAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await context.Users.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user is null)
        {
            return;
        }

        await SendEmailCodeAsync(user.Id, cancellationToken);
    }

    public async Task<TokensModel> VerifyEmailCodeAsync(string email, string code, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);
        var now = timeProvider.GetUtcNow();

        var user = await context.Users.SingleOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidEmailVerificationCode);
        }

        if (user.IsBlocked)
        {
            throw new UnauthorizedException(UnauthorizedErrors.UserBlocked);
        }

        if (user.EmailVerified)
        {
            throw new UnauthorizedException(UnauthorizedErrors.EmailAlreadyVerified);
        }

        var verificationCode = await context.VerificationCodes
            .Where(x =>
                x.UserId == user.Id
                && x.Purpose == VerificationCodePurpose.EmailVerification
                && x.Destination == normalizedEmail
                && x.UsedAt == null)
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(cancellationToken);

        if (verificationCode is null
            || verificationCode.ExpiresAt <= now
            || verificationCode.FailedAttempts >= MaxEmailCodeAttempts)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidEmailVerificationCode);
        }

        if (!tokenHasher.VerifyToken(code, verificationCode.CodeHash))
        {
            verificationCode.FailedAttempts++;
            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(UnauthorizedErrors.InvalidEmailVerificationCode);
        }

        user.EmailVerified = true;
        verificationCode.UsedAt = now;

        return await authTokenIssuer.IssueTokensAsync(user, cancellationToken);
    }

    private static void RegisterFailedLogin(User user, DateTimeOffset now)
    {
        user.AccessFailedCount++;

        if (user.AccessFailedCount < MaxAttempts) return;
        
        user.LockoutEnd = now.Add(LockoutDuration);
        user.AccessFailedCount = 0;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private async Task SendEmailVerificationCodeEmailAsync(
        User user,
        string code,
        CancellationToken cancellationToken)
    {
        await emailSender.SendEmailAsync(
            user.Email,
            user.Email,
            "Verify your email",
            $"Your email verification code is {code}. It expires in 15 minutes.",
            cancellationToken);
    }

    private async Task SendPasswordResetCodeEmailAsync(
        User user,
        string code,
        CancellationToken cancellationToken)
    {
        await emailSender.SendEmailAsync(
            user.Email,
            user.Email,
            "Reset your password",
            $"Your password reset code is {code}. It expires in 15 minutes.",
            cancellationToken);
    }

    private static T GetRequiredClaim<T>(
        JwtSecurityToken jwtToken,
        string claimType,
        TryParse<T> tryParse)
    {
        var value = jwtToken.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;

        if (value is null || !tryParse(value, out var result))
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidRefreshToken);
        }

        return result;
    }

    private static void RevokeSession(UserSession session, DateTimeOffset now, string reason)
    {
        session.RevokedAt ??= now;
        session.RevokedReason ??= reason;
        session.LastUsedAt = now;

        foreach (var refreshToken in session.RefreshTokens.Where(x => !x.RevokedAt.HasValue))
        {
            refreshToken.RevokedAt = now;
            refreshToken.RevokedReason = reason;
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }

    private async Task<bool> CanSendVerificationCodeAsync(
        int userId,
        string destination,
        VerificationCodePurpose purpose,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var cooldownStartedAt = now.Subtract(CodeSendCooldown);
        var limitWindowStartedAt = now.Subtract(CodeSendLimitWindow);

        var sentDuringCooldown = await context.VerificationCodes.AnyAsync(
            x => x.UserId == userId
                 && x.Purpose == purpose
                 && x.Destination == destination
                 && x.Created >= cooldownStartedAt,
            cancellationToken);

        if (sentDuringCooldown)
        {
            return false;
        }

        var sentDuringLimitWindow = await context.VerificationCodes.CountAsync(
            x => x.UserId == userId
                 && x.Purpose == purpose
                 && x.Destination == destination
                 && x.Created >= limitWindowStartedAt,
            cancellationToken);

        return sentDuringLimitWindow < MaxCodeSendAttemptsPerHour;
    }

    private async Task InvalidateUnusedVerificationCodesAsync(
        int userId,
        string destination,
        VerificationCodePurpose purpose,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await context.VerificationCodes
            .Where(x => x.UserId == userId
                        && x.Purpose == purpose
                        && x.Destination == destination
                        && x.UsedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.UsedAt, now),
                cancellationToken);
    }

    private delegate bool TryParse<T>(string value, out T result);
}
