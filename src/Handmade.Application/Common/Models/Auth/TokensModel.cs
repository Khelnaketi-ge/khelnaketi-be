namespace Handmade.Application.Common.Models.Auth;

public sealed record TokensModel(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
