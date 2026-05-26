using System.Security.Claims;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Features.Auth.Commands.ExternalLogin;
using Handmade.Application.Features.Auth.Commands.PanelExternalLogin;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace Handmade.WebApi.Services;

public interface IGoogleAuthService
{
    AuthenticationProperties CreateChallengeProperties(string? redirectUri, string? returnUrl = null);
    Task<TokensModel?> HandleCallbackAsync(bool requireBrandOwner, CancellationToken cancellationToken);
    string? GetReturnUrl();
}

public sealed class GoogleAuthService(
    IHttpContextAccessor httpContextAccessor,
    ISender sender) : IGoogleAuthService
{
    private const string ReturnUrlKey = "returnUrl";
    private string? returnUrl;

    public AuthenticationProperties CreateChallengeProperties(string? redirectUri, string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            properties.Items[ReturnUrlKey] = returnUrl;
        }

        return properties;
    }

    public async Task<TokensModel?> HandleCallbackAsync(bool requireBrandOwner, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required for Google authentication.");

        var result = await httpContext.AuthenticateAsync("External");

        if (!result.Succeeded || result.Principal is null)
        {
            return null;
        }

        returnUrl = GetReturnUrl(result.Properties);

        var providerUserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var displayName = result.Principal.FindFirstValue(ClaimTypes.Name);
        var emailVerified = string.Equals(
            result.Principal.FindFirstValue("email_verified"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        await httpContext.SignOutAsync("External");

        if (requireBrandOwner)
        {
            return await sender.Send(
                new PanelExternalLoginCommand(
                    Provider.Google,
                    providerUserId ?? string.Empty,
                    email,
                    emailVerified,
                    displayName),
                cancellationToken);
        }

        return await sender.Send(
            new ExternalLoginCommand(
                Provider.Google,
                providerUserId ?? string.Empty,
                email,
                emailVerified,
                displayName),
            cancellationToken);
    }

    public string? GetReturnUrl()
    {
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return returnUrl;
        }

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required for Google authentication.");

        var properties = httpContext.Features.Get<IAuthenticateResultFeature>()?
            .AuthenticateResult?
            .Properties;

        return GetReturnUrl(properties);
    }

    private static string? GetReturnUrl(AuthenticationProperties? properties)
    {
        return properties?.Items is not null
            && properties.Items.TryGetValue(ReturnUrlKey, out var returnUrl)
            ? returnUrl
            : null;
    }
}
