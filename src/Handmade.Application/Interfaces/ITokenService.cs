using System.IdentityModel.Tokens.Jwt;
using Handmade.Domain.Entities;

namespace Handmade.Application.Interfaces;

public interface ITokenService
{
    (string jwtToken, DateTimeOffset expiresAt) CreateJwtToken(
        User user,
        Guid sessionId,
        bool ownsBrand);

    (string refreshToken, DateTimeOffset expiresAt) GenerateRefreshToken();
    bool ValidateToken(string token, out JwtSecurityToken jwtToken, bool validateLifetime = true);
}
