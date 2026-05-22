using System.Reflection;
using FluentValidation;
using Handmade.Application.Common.Behaviours;
using Mapster;
using Mapster.Utils;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Handmade.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Mapster
        TypeAdapterConfig.GlobalSettings
            .ScanInheritedTypes(Assembly.GetExecutingAssembly());

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });
    }
}