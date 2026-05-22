namespace Handmade.Infrastructure.Options;

public sealed class JwtOptions
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenTtl { get; set; }
    public int RefreshTokenTtl { get; set; }
}
