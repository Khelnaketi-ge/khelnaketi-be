using System.Text;
using Handmade.Infrastructure.Options;
using Microsoft.IdentityModel.Tokens;

namespace Handmade.Infrastructure.Auth;

public static class JwtTokenValidationParametersFactory
{
    public static TokenValidationParameters Create(JwtOptions jwtOptions, bool validateLifetime = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Audience);

        return new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    }
}
