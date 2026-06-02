using Asp.Versioning;
using System.Globalization;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Features.Auth.Commands.PanelLogin;
using Handmade.WebApi.Infrastructure;
using Handmade.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/panel/auth")]
public class PanelAuthController(
    ISender sender,
    IGoogleAuthService googleAuthService) : ApiController(sender)
{
    private const string DefaultFrontendLoginUrl = "http://localhost:3000/login";
    private const string GeneralGoogleAuthError = "InvalidExternalLogin";

    [HttpPost("login")]
    [EnableRateLimiting(RateLimiterPolicies.AuthSensitive)]
    public async Task<IActionResult> Login(
        [FromBody] PanelLoginCommand command, CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpGet("external/google")]
    public IActionResult Google([FromQuery] string? returnUrl)
    {
        var redirectUrl = Url.ActionLink(nameof(GoogleCallback));
        var properties = googleAuthService.CreateChallengeProperties(
            redirectUrl,
            NormalizeFrontendReturnUrl(returnUrl));

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("external/google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        try
        {
            var tokens = await googleAuthService.HandleCallbackAsync(requireBrandOwner: true, HttpContext.RequestAborted);

            return tokens is null ? RedirectWithError() : RedirectWithTokens(tokens);
        }
        catch (UnauthorizedException exception)
        {
            return RedirectWithError(exception.Code, exception.Message);
        }
    }

    private RedirectResult RedirectWithError(string? code = null, string? detail = null)
    {
        var returnUrl = NormalizeFrontendReturnUrl(googleAuthService.GetReturnUrl());
        var redirectUrl = QueryHelpers.AddQueryString(
            returnUrl,
            new Dictionary<string, string?>
            {
                ["error"] = string.IsNullOrWhiteSpace(code) ? GeneralGoogleAuthError : code,
                ["message"] = string.IsNullOrWhiteSpace(detail) ? "Invalid external login" : detail
            });

        return Redirect(redirectUrl);
    }

    private RedirectResult RedirectWithTokens(TokensModel tokens)
    {
        var returnUrl = NormalizeFrontendReturnUrl(googleAuthService.GetReturnUrl());
        var redirectUrl = QueryHelpers.AddQueryString(
            returnUrl,
            new Dictionary<string, string?>
            {
                ["accessToken"] = tokens.AccessToken,
                ["accessTokenExpiresAt"] = tokens.AccessTokenExpiresAt.ToString("O", CultureInfo.InvariantCulture),
                ["refreshToken"] = tokens.RefreshToken,
                ["refreshTokenExpiresAt"] = tokens.RefreshTokenExpiresAt.ToString("O", CultureInfo.InvariantCulture)
            });

        return Redirect(redirectUrl);
    }

    private static string NormalizeFrontendReturnUrl(string? returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)
            || uri.Host != "localhost"
            || uri.Port is not (3000 or 3001)
            || uri.Scheme is not ("http" or "https"))
        {
            return DefaultFrontendLoginUrl;
        }

        return uri.ToString();
    }
}
