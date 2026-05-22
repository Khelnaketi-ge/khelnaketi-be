using System.IdentityModel.Tokens.Jwt;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;

namespace Handmade.Application.Interfaces;

public interface ITokenService
{
    (string jwtToken, DateTimeOffset expiresAt) CreateJwtToken(
        User user,
        Guid sessionId,
        IEnumerable<Permissions>? permissions = null);

    (string refreshToken, DateTimeOffset expiresAt) GenerateRefreshToken();
    bool ValidateToken(string token, out JwtSecurityToken jwtToken, bool validateLifetime = true);
}
