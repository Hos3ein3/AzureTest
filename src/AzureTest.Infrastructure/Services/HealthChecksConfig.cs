

using AzureTest.Infrastructure.Configuration;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Services;

public static class HealthChecksConfig
{

    public static IEndpointRouteBuilder EnableHealthChecks(this IEndpointRouteBuilder endpoints,InfrastructureFeatureOptions options)
    {
        if (options.EnableHealthCheck)
        {
           

        }
        return endpoints;
    }
    public static IServiceCollection AddHealthChecksConfig(
        this IServiceCollection services,
        InfrastructureFeatureOptions options,
        IConfiguration configuration)
    {
        if (!options.EnableHealthCheck)
            return services;

        var healthChecksBuilder = services.AddHealthChecks();

        // 1. PostgreSQL
        if (options.EnableHealthChecksSql)
        {
            var sqlServer = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(sqlServer))
                healthChecksBuilder.AddNpgSql(sqlServer, name: "PostgreSQL", tags: new[] { "db", "sql", "ready" });
        }

        // 2. MongoDB
        if (options.EnableHealthChecksCosmos)
        {
            var mongoDb = configuration.GetConnectionString("MongoDb");
            if (!string.IsNullOrWhiteSpace(mongoDb))
                healthChecksBuilder.AddMongoDb(
                    _ => new MongoDB.Driver.MongoClient(mongoDb),
                    name: "MongoDB",
                    tags: new[] { "db", "nosql", "ready" });
        }
        
        // 3. Redis
        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
            healthChecksBuilder.AddRedis(redis, name: "Redis", tags: new[] { "cache", "ready" });
            
        // 4. RabbitMQ (Only if enabled in FeatureManagement)
        var enableRabbitMQ = configuration.GetValue<bool>("FeatureManagement:EnableRabbitMQ");
        if (enableRabbitMQ)
        {
            var rabbitmq = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@rabbitmq:5672";
            healthChecksBuilder.AddRabbitMQ(rabbitmq, name: "RabbitMQ", tags: new[] { "messaging", "ready" });
        }

        // 5. Hangfire (Only if enabled in FeatureManagement)
        var enableHangfire = configuration.GetValue<bool>("FeatureManagement:EnableHangfire");
        if (enableHangfire)
        {
            healthChecksBuilder.AddHangfire(hangfireOptions => {
                hangfireOptions.MinimumAvailableServers = 1;
            }, name: "Hangfire", tags: new[] { "jobs", "ready" });
        }

        // 6. Azure Blob Storage (Only if enabled in FeatureManagement & configured)
        if (options.EnableHealthChecksBlob && options.EnableBlobStorage)
        {
            var blobStorage = configuration.GetConnectionString("BlobStorage");
            if (!string.IsNullOrWhiteSpace(blobStorage))
                healthChecksBuilder.AddAzureBlobStorage(blobStorage, name: "AzureBlob", tags: new[] { "storage" });
        }

        // 7. Application Insights Publisher (Only if enabled and configured)
        if (options.EnableHealthCheckMonitor && options.EnableAzureMonitorOpenTelemetry)
        {
            var appInsights = configuration.GetSection("AzureMonitor:ConnectionString").Value;
            if (!string.IsNullOrWhiteSpace(appInsights))
            {
                services.Configure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckPublisherOptions>(opts =>
                {
                    opts.Delay = TimeSpan.FromSeconds(5);
                    opts.Predicate = (check) => true;
                });
                services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config => 
                {
                    config.ConnectionString = appInsights;
                });
                healthChecksBuilder.AddApplicationInsightsPublisher();
            }
        }

        // 8. Seq (Only if enabled in FeatureManagement & configured)
        if (options.UseSeq)
        {
            var seqUrl = configuration["SeqUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl) && Uri.TryCreate(seqUrl, UriKind.Absolute, out var baseUri))
            {
                var seqHealthUri = new Uri(new Uri(seqUrl.TrimEnd('/') + "/"), "health");
                healthChecksBuilder.AddUrlGroup(seqHealthUri, name: "Seq", tags: new[] { "logging", "ready" }, timeout: TimeSpan.FromSeconds(3));
            }
        }

        return services;
    }
}
