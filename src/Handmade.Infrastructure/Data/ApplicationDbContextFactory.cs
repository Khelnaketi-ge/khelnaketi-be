using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Handmade.Infrastructure.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string GetConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Supabase");

        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var configPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Handmade.WebApi",
            "appsettings.Development.json"));

        using var configFile = File.OpenRead(configPath);
        using var configJson = JsonDocument.Parse(configFile);

        if (configJson.RootElement
                .GetProperty("ConnectionStrings")
                .TryGetProperty("Supabase", out var connectionStringElement)
            && !string.IsNullOrWhiteSpace(connectionStringElement.GetString()))
        {
            return connectionStringElement.GetString()!;
        }

        throw new InvalidOperationException("Connection string 'Supabase' not found.");
    }
}
