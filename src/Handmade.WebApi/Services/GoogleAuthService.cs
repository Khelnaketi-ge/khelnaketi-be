using System.Security.Claims;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Features.Auth.Commands.ExternalLogin;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace Handmade.WebApi.Services;

public interface IGoogleAuthService
{
    AuthenticationProperties CreateChallengeProperties(string? redirectUri);
    Task<TokensModel?> HandleCallbackAsync(CancellationToken cancellationToken);
}

public sealed class GoogleAuthService(
    IHttpContextAccessor httpContextAccessor,
    ISender sender) : IGoogleAuthService
{
    public AuthenticationProperties CreateChallengeProperties(string? redirectUri)
    {
        return new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };
    }

    public async Task<TokensModel?> HandleCallbackAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required for Google authentication.");

        var result = await httpContext.AuthenticateAsync("External");

        if (!result.Succeeded || result.Principal is null)
        {
            return null;
        }

        var providerUserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var displayName = result.Principal.FindFirstValue(ClaimTypes.Name);
        var emailVerified = string.Equals(
            result.Principal.FindFirstValue("email_verified"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        await httpContext.SignOutAsync("External");

        return await sender.Send(
            new ExternalLoginCommand(
                Provider.Google,
                providerUserId ?? string.Empty,
                email,
                emailVerified,
                displayName),
            cancellationToken);
    }
}
