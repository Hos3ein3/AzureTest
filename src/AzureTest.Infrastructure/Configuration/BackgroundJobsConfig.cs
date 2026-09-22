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
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                }));

            // Add the processing server as IHostedService
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 5;
            });
        }

        // ── Quartz (In-Memory / Fast Scheduling) ─────────────────────────────────
        services.AddQuartz(q =>
        {
            // You can register Quartz jobs here later:
            // q.ScheduleJob<MyJob>(trigger => trigger
            //     .WithIdentity("MyJobTrigger")
            //     .StartNow()
            //     .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
        });

        // Quartz hosted service
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
