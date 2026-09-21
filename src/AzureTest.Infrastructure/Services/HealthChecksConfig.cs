using AzureTest.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
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

        var connections = configuration
            .GetSection("HealthCheckConnections")
            .Get<HealthCheckConnectionOptions>() ?? new HealthCheckConnectionOptions();

        var healthChecksBuilder = services.AddHealthChecks();

        if (options.EnableHealthChecksSql && !string.IsNullOrWhiteSpace(connections.SqlServer))
        {
            healthChecksBuilder.AddSqlServer(connections.SqlServer, name: "AzureSql");
        }

        if (options.EnableHealthChecksCosmos && !string.IsNullOrWhiteSpace(connections.CosmosDb))
        {
            // Register CosmosClient in DI, then health check resolves it automatically
            services.AddSingleton(_ => new CosmosClient(connections.CosmosDb));
            healthChecksBuilder.AddAzureCosmosDB(name: "AzureCosmos");
        }

        if (options.EnableHealthChecksBlob && !string.IsNullOrWhiteSpace(connections.BlobStorage))
        {
            healthChecksBuilder.AddAzureBlobStorage(connections.BlobStorage, name: "AzureBlob");
        }

        return services;
    }
}
