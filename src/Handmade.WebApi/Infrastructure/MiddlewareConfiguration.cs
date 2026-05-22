using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;

namespace Handmade.WebApi.Infrastructure;

public static class MiddlewareConfiguration
{
    public static void ConfigureMiddlewares(this WebApplication app)
    {
        // Localization
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ru-RU"),
            new CultureInfo("ka-GE")
        };

        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        };

        localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
        
        app.UseExceptionHandler();

        // Dev
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        // Https
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        
        // Middlewares
        app.UseRequestLocalization(localizationOptions);
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
