using Asp.Versioning;
using System.Globalization;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Features.Auth.Commands.Login;
using Handmade.Application.Features.Auth.Commands.Refresh;
using Handmade.Application.Features.Auth.Commands.Register;
using Handmade.Application.Features.Auth.Commands.ResendVerificationCode;
using Handmade.Application.Features.Auth.Commands.RequestPasswordReset;
using Handmade.Application.Features.Auth.Commands.ResetPassword;
using Handmade.Application.Features.Auth.Commands.VerifyEmail;
using Handmade.WebApi.Infrastructure;
using Handmade.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    ISender sender,
    IGoogleAuthService googleAuthService) : ApiController(sender)
{
    private const string DefaultFrontendReturnUrl = "http://localhost:3000/ka/auth/google/callback";
    private const string GeneralGoogleAuthError = "InvalidExternalLogin";

    [HttpPost("register")]
    [EnableRateLimiting(RateLimiterPolicies.AuthEmail)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting(RateLimiterPolicies.AuthSensitive)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimiterPolicies.AuthSensitive)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshCommand command, CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPost("password-reset/request")]
    [EnableRateLimiting(RateLimiterPolicies.AuthEmail)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("codes/resend")]
    [EnableRateLimiting(RateLimiterPolicies.AuthEmail)]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("password-reset/confirm")]
    [EnableRateLimiting(RateLimiterPolicies.AuthSensitive)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
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
            var tokens = await googleAuthService.HandleCallbackAsync(requireBrandOwner: false, HttpContext.RequestAborted);

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
            || uri.Port is not (3000 or 3002)
            || uri.Scheme is not ("http" or "https"))
        {
            return DefaultFrontendReturnUrl;
        }

        return uri.ToString();
    }
}
