using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Handmade.Shared.Extensions;

public static class DiExtensions
{
    public static IServiceCollection ConfigureOptionsAsSingleton<T>(this IServiceCollection services, IConfigurationSection configurationSection) where T : class
    {
        services.Configure<T>(configurationSection);
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<T>>().Value);
        return services;
    }
}