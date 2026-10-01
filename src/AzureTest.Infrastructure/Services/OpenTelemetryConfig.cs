

using AzureTest.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AzureTest.Infrastructure.Services;

public static class OpenTelemetryConfig
{
    public static IServiceCollection AddOpenTelemetryConfig(this IServiceCollection services, InfrastructureFeatureOptions options)
    {
        if (!options.EnableOpenTelemetry)
            return services;

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                if (options.EnableAspNetCoreInstrumentation)
                    tracing.AddAspNetCoreInstrumentation();

                tracing.AddHttpClientInstrumentation();

                tracing.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri("http://tempo:4317");
                });
            })
            .WithMetrics(metrics =>
            {
                if (options.EnableAspNetCoreInstrumentation)
                    metrics.AddAspNetCoreInstrumentation();
                if (options.EnableRuntimeInstrumentation)
                    metrics.AddRuntimeInstrumentation();
                if (options.EnableProcessInstrumentation)
                    metrics.AddProcessInstrumentation();

                metrics.AddPrometheusExporter();
            });

        return services;
    }
}
