namespace Handmade.Application.Common.Models.Auth;

public sealed record AccessTokenValidationResult(bool Succeeded, string? FailureMessage)
{
    public static AccessTokenValidationResult Success() => new(true, null);

    public static AccessTokenValidationResult Failure(string message) => new(false, message);
}
