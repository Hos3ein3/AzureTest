

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Configuration;

public static class MessageBrokerConfig
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        var enableRabbitMQ = configuration.GetValue<bool>("FeatureManagement:EnableRabbitMQ");

        services.AddMassTransit(x =>
        {
            
            if (enableRabbitMQ)
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
