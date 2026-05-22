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
using Microsoft.EntityFrameworkCore;
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

            options.AddPolicy("AuthSensitive", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("AuthEmail", httpContext =>
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
                        var principal = context.Principal;

                        if (!int.TryParse(principal?.FindFirst(Claims.Id)?.Value, out var userId))
                        {
                            context.Fail("Missing or invalid user id claim.");
                            return;
                        }

                        if (!Guid.TryParse(principal.FindFirst(Claims.SessionId)?.Value, out var sessionId))
                        {
                            context.Fail("Missing or invalid session id claim.");
                            return;
                        }

                        if (!int.TryParse(principal.FindFirst(Claims.TokenVersion)?.Value, out var tokenVersion))
                        {
                            context.Fail("Missing or invalid token version claim.");
                            return;
                        }

                        if (!int.TryParse(principal.FindFirst(Claims.PermissionVersion)?.Value, out var permissionVersion))
                        {
                            context.Fail("Missing or invalid permission version claim.");
                            return;
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
                        var cancellationToken = context.HttpContext.RequestAborted;

                        var user = await dbContext.Users
                            .AsNoTracking()
                            .Where(x => x.Id == userId)
                            .Select(x => new
                            {
                                x.Id,
                                x.IsBlocked,
                                x.TokenVersion,
                                x.PermissionVersion
                            })
                            .FirstOrDefaultAsync(cancellationToken);

                        if (user is null)
                        {
                            context.Fail("User does not exist.");
                            return;
                        }

                        if (user.IsBlocked)
                        {
                            context.Fail("User is blocked.");
                            return;
                        }

                        if (user.TokenVersion != tokenVersion)
                        {
                            context.Fail("Token version is invalid.");
                            return;
                        }

                        if (user.PermissionVersion != permissionVersion)
                        {
                            context.Fail("Permission version is invalid.");
                            return;
                        }

                        var session = await dbContext.UserSessions
                            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);

                        if (session is null)
                        {
                            context.Fail("Session does not exist.");
                            return;
                        }

                        if (session.RevokedAt.HasValue)
                        {
                            context.Fail("Session is revoked.");
                            return;
                        }

                        var timeProvider = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>();
                        var now = timeProvider.GetUtcNow();

                        if (!session.LastUsedAt.HasValue || session.LastUsedAt.Value.AddMinutes(1) <= now)
                        {
                            session.LastUsedAt = now;
                            await dbContext.SaveChangesAsync(cancellationToken);
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
