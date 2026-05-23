using Asp.Versioning;
using Handmade.Application.Features.Auth.Commands.Login;
using Handmade.Application.Features.Auth.Commands.Logout;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    ISender sender,
    IGoogleAuthService googleAuthService) : ApiController(sender)
{
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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutCommand(), cancellationToken);
        return NoContent();
    }

    [HttpGet("external/google")]
    public IActionResult Google()
    {
        var redirectUrl = Url.ActionLink(nameof(GoogleCallback));
        var properties = googleAuthService.CreateChallengeProperties(redirectUrl);

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("external/google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var tokens = await googleAuthService.HandleCallbackAsync(HttpContext.RequestAborted);

        if (tokens is null)
        {
            return Unauthorized(new
            {
                message = "Google authentication failed"
            });
        }

        return Ok(tokens);
    }
}
