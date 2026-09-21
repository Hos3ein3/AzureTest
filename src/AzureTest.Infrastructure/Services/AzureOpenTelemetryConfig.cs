using Azure.Monitor.OpenTelemetry.AspNetCore;

using AzureTest.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AzureTest.Infrastructure.Services;

public static class AzureOpenTelemetryConfig
{
    public static IServiceCollection AddAzureOpenTelemetryConfig(this IServiceCollection services, InfrastructureFeatureOptions options)
    {
        if (!options.EnableAzureMonitorOpenTelemetry)
            return services;

        services.AddOpenTelemetry().UseAzureMonitor();

        return services;
    }
} 