using AzureTest.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace AzureTest.Infrastructure.Services;

public static class SerilogConfig
{
    public static IServiceCollection AddSerilogConfig(this IServiceCollection services, InfrastructureFeatureOptions options)
    {
        if (!options.EnableSerilog)
        {
            return services;
        }

        services.AddSerilog((provider, loggerConfiguration) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .ReadFrom.Configuration(configuration);
        });

        return services;
    }
}
