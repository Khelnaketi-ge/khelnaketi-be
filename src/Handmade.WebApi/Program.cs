using Handmade.Application;
using Handmade.Infrastructure;
using Handmade.WebApi.Infrastructure;

namespace Handmade.WebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddInfrastructureServices();
        builder.AddApplicationServices();
        builder.AddWebApiServices();
        
        var app = builder.Build();

        app.ConfigureMiddlewares();
        app.Run();
    }
}