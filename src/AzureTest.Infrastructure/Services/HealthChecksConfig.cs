using AzureTest.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Services;

public static class HealthChecksConfig
{
    public static IServiceCollection AddHealthChecksConfig(
        this IServiceCollection services,
        InfrastructureFeatureOptions options,
        IConfiguration configuration)
    {
        if (!options.EnableHealthCheck)
            return services;

        var healthChecksBuilder = services.AddHealthChecks();

        if (options.EnableHealthChecksSql)
        {
            var sqlServer = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(sqlServer))
                healthChecksBuilder.AddNpgSql(sqlServer, name: "PostgreSQL");
        }

        if (options.EnableHealthChecksCosmos)
        {
            var mongoDb = configuration.GetConnectionString("MongoDb");
            if (!string.IsNullOrWhiteSpace(mongoDb))
                healthChecksBuilder.AddMongoDb(
                    _ => new MongoDB.Driver.MongoClient(mongoDb),
                    name: "MongoDB");
        }
        
        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
            healthChecksBuilder.AddRedis(redis, name: "Redis");
            
        // RabbitMQ (Assuming default Docker credentials/port if not in config)
        var rabbitmq = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@rabbitmq:5672";
        healthChecksBuilder.AddRabbitMQ(rabbitmq, name: "RabbitMQ");
        
        // Grafana
        healthChecksBuilder.AddUrlGroup(new Uri("http://grafana:3000/api/health"), name: "Grafana");

        // Hangfire
        healthChecksBuilder.AddHangfire(options => {
            options.MinimumAvailableServers = 1;
        }, name: "Hangfire");

        if (options.EnableHealthChecksBlob)
        {
            var blobStorage = configuration.GetConnectionString("BlobStorage");
            if (!string.IsNullOrWhiteSpace(blobStorage))
                healthChecksBuilder.AddAzureBlobStorage(blobStorage, name: "AzureBlob");
        }

        if (options.EnableHealthCheckMonitor)
        {
            // Add Application Insights Publisher if Azure Monitor is enabled
            var appInsights = configuration.GetSection("AzureMonitor:ConnectionString").Value;
            if (!string.IsNullOrWhiteSpace(appInsights))
            {
                services.Configure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckPublisherOptions>(opts =>
                {
                    opts.Delay = TimeSpan.FromSeconds(2);
                    opts.Predicate = (check) => true;
                });
                services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config => 
                {
                    config.ConnectionString = appInsights;
                });
                healthChecksBuilder.AddApplicationInsightsPublisher();
            }
        }
        

        return services;
    }
}
