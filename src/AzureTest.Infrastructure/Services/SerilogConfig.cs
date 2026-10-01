

using AzureTest.Infrastructure.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

namespace AzureTest.Infrastructure.Services;

public static class SerilogConfig
{
    public static IServiceCollection AddSerilogConfig(this IServiceCollection services,
        InfrastructureFeatureOptions options)
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
                .Enrich.WithSpan() 
                //.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
                //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .ReadFrom.Configuration(configuration);

            if (options.UseSeq)
            {
                var seqUrl = configuration["SeqUrl"];
                if (!string.IsNullOrWhiteSpace(seqUrl))
                {
                    loggerConfiguration.WriteTo.Seq(seqUrl);
                }
            }
        });

        return services;
    }
}