using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        InfrastructureFeatureOptions options,
        IConfiguration configuration)
    {
        services.AddSerilogConfig(options);
        services.AddOpenTelemetryConfig(options);
        services.AddHealthChecksConfig(options, configuration);
        services.AddAzureOpenTelemetryConfig(options);

        // Register the correct storage implementation based on the feature flag
        if (options.EnableBlobStorage)
        {
            services.AddScoped<IBlobService, BlobService>();
        }
        else
        {
            services.AddScoped<IBlobService, LocalFileService>();
        }

        return services;
    }
}

