using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace AzureTest.Infrastructure.Configuration;

public static class BackgroundJobsConfig
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // ── Hangfire (Persistent Jobs) ───────────────────────────────────────────
        var enableHangfire = configuration.GetValue<bool>("FeatureManagement:EnableHangfire");
        if (enableHangfire && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddHangfire((sp, config) => {
                var currentConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options =>
                    {
                        options.UseNpgsqlConnection(currentConnectionString);
                    });
            });

            // Add the processing server as IHostedService
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 5;
            });
        }

        // ── Quartz (In-Memory / Fast Scheduling) ─────────────────────────────────
        var enableQuartz = configuration.GetValue<bool>("FeatureManagement:EnableQuartz");
        if (enableQuartz)
        {
            services.AddQuartz(q =>
            {
                // You can register Quartz jobs here later:
            });

            // Quartz hosted service
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }

        return services;
    }
}
