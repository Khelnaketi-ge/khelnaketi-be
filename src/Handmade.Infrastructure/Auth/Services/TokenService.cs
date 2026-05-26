using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace Handmade.Infrastructure.Auth.Services;

public class TokenService(JwtOptions jwtOptions, TimeProvider timeProvider) : ITokenService
{
    public (string jwtToken, DateTimeOffset expiresAt) CreateJwtToken(
        User user,
        Guid sessionId,
        bool ownsBrand)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Audience);

        if (user.Id <= 0)
        {
            throw new ArgumentException("User must have an id to create a JWT.", nameof(user));
        }

        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session id is required to create a JWT.", nameof(sessionId));
        }

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(jwtOptions.AccessTokenTtl);

        var claims = new List<System.Security.Claims.Claim>
        {
            new(Claims.Id, user.Id.ToString()),
            new(Claims.SessionId, sessionId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new("email", user.Email),
            new(Claims.AccessLevel, ((int)user.AccessLevel).ToString()),
            new(Claims.SuperAdmin, ToFlag(user.AccessLevel == AccessLevel.SuperAdmin)),
            new(Claims.BrandOwner, ToFlag(ownsBrand)),
            new(Claims.EmailVerified, ToFlag(user.EmailVerified)),
            new(Claims.PhoneVerified, ToFlag(user.PhoneNumberVerified)),
            new(Claims.TokenVersion, user.TokenVersion.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            NotBefore = now.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var jwtToken = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler().CreateToken(tokenDescriptor);
        return (jwtToken, expiresAt);
    }
    
    
    public (string refreshToken, DateTimeOffset expiresAt) GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var token = Convert.ToBase64String(randomNumber);
        var expires = timeProvider.GetUtcNow().AddMinutes(jwtOptions.RefreshTokenTtl);

        return (token, expires);
    }

    public bool ValidateToken(string token, out JwtSecurityToken jwtToken, bool validateLifetime = true)
    {
        jwtToken = null!;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
        {
            return false;
        }

        try
        {
            tokenHandler.ValidateToken(token, JwtTokenValidationParametersFactory.Create(jwtOptions, validateLifetime), out var validatedToken);

            if (validatedToken is not JwtSecurityToken validatedJwtToken
                || !string.Equals(
                    validatedJwtToken.Header.Alg,
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.Ordinal))
            {
                return false;
            }

            jwtToken = validatedJwtToken;
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string ToFlag(bool value) => value ? "1" : "0";
}
