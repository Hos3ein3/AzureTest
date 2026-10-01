

using AzureTest.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application;

public static class ScrutorConfig
{
    public static IServiceCollection AddScrutorConfig(this IServiceCollection services)
    {
        services.Scan(selector => selector
            .FromAssemblies(typeof(ScrutorConfig).Assembly)
            .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(filter => filter.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
