using Ardalis.GuardClauses;
using Asp.Versioning;
using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Auth;
using Handmade.Infrastructure.Auth.Hashers;
using Handmade.Infrastructure.Auth.Services;
using Handmade.Infrastructure.Data;
using Handmade.Infrastructure.Data.Interceptors;
using Handmade.Infrastructure.Options;
using Handmade.Infrastructure.Services;
using Handmade.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Handmade.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);

        // Options
        builder.Services.ConfigureOptionsAsSingleton<JwtOptions>
            (builder.Configuration.GetSection(nameof(JwtOptions)));
        builder.Services.ConfigureOptionsAsSingleton<SmtpOptions>(
            builder.Configuration.GetSection(nameof(SmtpOptions)));
        builder.Services.ConfigureOptionsAsSingleton<SupabaseStorage>(
            builder.Configuration.GetSection(nameof(SupabaseStorage)));
        
        // Connection string
        var connectionString = builder.Configuration.GetConnectionString("Supabase");
        Guard.Against.Null(connectionString, message: "Connection string 'Supabase' not found.");
        
        // Interceptors
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        
        // Database
        builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options
                .UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
        });
        
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        
        // Auth
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        builder.Services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<ITokenHasher, TokenHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddSingleton(_ => new HttpClient());
        builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
        
        // Versioning
        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = builder.Environment.IsDevelopment();
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
