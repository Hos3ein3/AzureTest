using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using System;

namespace AzureTest.Infrastructure.Configuration;

public static class ResilienceConfig
{
    public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
    {
        // 1. Retry 3 times in 3 seconds (each retry waits 3 seconds)
        services.AddResiliencePipeline("Retry3Times3Seconds", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(3),
                BackoffType = DelayBackoffType.Constant
            });
        });

        // 2. Retry 3 times in 5 minutes (each retry waits 5 minutes)
        services.AddResiliencePipeline("Retry3Times5Minutes", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMinutes(5),
                BackoffType = DelayBackoffType.Constant
            });
        });

        // 3. Add Standard HTTP Resilience (Requires Microsoft.Extensions.Http.Resilience package)
        services.ConfigureHttpClientDefaults(http =>
        {
            // Adds a standard resilience handler to all HttpClients
            // This includes standard retries, circuit breaker, and timeout.
            http.AddStandardResilienceHandler();
        });

        return services;
    }
}
