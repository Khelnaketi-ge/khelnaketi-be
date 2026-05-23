using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Auth;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.Infrastructure.Options;
using Handmade.WebApi.Infrastructure;
using Handmade.WebApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Handmade.WebApi;

public static class DependencyInjection
{
    public static void AddWebApiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimiterPolicies.AuthSensitive, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimiterPolicies.AuthEmail, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));
        });
        
        // Exception handling
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        
        // Current User
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        builder.Services.AddScoped<ICurrentRequest, CurrentRequest>();
        builder.Services.AddScoped<Services.IGoogleAuthService, Services.GoogleAuthService>();
        
        // problem details
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

                if (!context.ProblemDetails.Extensions.ContainsKey("code"))
                {
                    context.ProblemDetails.Extensions.TryAdd("code", context.ProblemDetails.Status.ToString());
                }
            };
        });
        
        // Auth
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddCookie("External", options =>
            {
                options.Cookie.Name = "Handmade.ExternalAuth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = "External";
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new ArgumentException("Authentication:Google:ClientId is required.");
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new ArgumentException("Authentication:Google:ClientSecret is required.");
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
                options.Events.OnCreatingTicket = context =>
                {
                    if (context.User.TryGetProperty("email_verified", out var emailVerified))
                    {
                        context.Identity?.AddClaim(new Claim("email_verified", emailVerified.ToString()));
                    }

                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(options =>
            {
                var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions))
                    .Get<JwtOptions>() ?? throw new ArgumentException(nameof(JwtOptions));

                options.TokenValidationParameters = JwtTokenValidationParametersFactory.Create(jwtOptions);
                options.MapInboundClaims = false;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var accessTokenValidator =
                            context.HttpContext.RequestServices.GetRequiredService<IAccessTokenValidator>();
                        var result = await accessTokenValidator.ValidateAsync(
                            context.Principal,
                            context.HttpContext.RequestAborted);

                        if (!result.Succeeded)
                        {
                            context.Fail(result.FailureMessage ?? "Access token validation failed.");
                        }
                    }
                };
            });
        
        builder.Services.AddAuthorization();
    }

    private static string GetClientIp(HttpContext httpContext)
    {
        var cloudflareIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cloudflareIp))
        {
            return cloudflareIp;
        }

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
