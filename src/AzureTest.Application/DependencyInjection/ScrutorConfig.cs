

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
                .AsImplementedInterfaces().AsSelf()
                .WithScopedLifetime()
            .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces().AsSelf()
                .WithScopedLifetime()
            .AddClasses(filter => filter.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces().AsSelf()
                .WithScopedLifetime()
        );

        // Decorate all CommandHandlers and QueryHandlers with automatic Audit Logging
        services.TryDecorate(typeof(ICommandHandler<>), typeof(AzureTest.Application.Behaviors.AuditCommandHandlerDecorator<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(AzureTest.Application.Behaviors.AuditCommandHandlerDecorator<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(AzureTest.Application.Behaviors.AuditQueryHandlerDecorator<,>));

        return services;
    }
}
