namespace Handmade.Infrastructure.Options;

public class GoogleAuthOptions
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string CallbackPath { get; init; } = "/signin-google";
}