

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Configuration;

public static class MessageBrokerConfig
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        var enableRabbitMQ = configuration.GetValue<bool>("FeatureManagement:EnableRabbitMQ");

        if (enableRabbitMQ)
        {
            services.AddMassTransit(x =>
            {
                // Register consumers here when needed, e.g.:
                // x.AddConsumer<MyConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        return services;
    }
}
