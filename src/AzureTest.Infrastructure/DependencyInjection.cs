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
        // services.AddScoped<IPdfGenerator, QuestPdfGenerator>();
        // services.AddScoped<IIdentityService, IdentityService>();
        
        services.AddScoped<INotificationService, MockNotificationService>();
        
        // ── Crawler Service ───────────────────────────────────────────────────
        services.AddHttpClient<IIndCrawlerService, IndCrawlerService>();

        // ── Caching ───────────────────────────────────────────────────────────
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
            services.AddScoped<ICompanyCacheService, CompanyCacheService>();
        }

        services.AddSerilogConfig(options);
        services.AddOpenTelemetryConfig(options);
        services.AddHealthChecksConfig(options, configuration);
        services.AddAzureOpenTelemetryConfig(options);
        services.AddBackgroundJobs(configuration);
        services.AddMessageBroker(configuration);
        services.AddResiliencePipelines();

        // Register the correct storage implementation based on the feature flag
        if (options.EnableBlobStorage)
        {
            services.AddScoped<IBlobService, BlobService>();
        }
        else
        {
            services.AddScoped<IBlobService, LocalFileService>();
        }

        // Register User and Role Management Services
        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddScoped<IRoleManagerService, RoleManagerService>();

        // Register CurrentUser and HttpContextAccessor
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddTransient<AzureTest.Infrastructure.Seeding.IdentityDataSeeder>();

        services.Configure<AzureTest.Infrastructure.Configuration.JwtSettings>(configuration.GetSection(AzureTest.Infrastructure.Configuration.JwtSettings.SectionName));
        services.Configure<InfrastructureFeatureOptions>(configuration.GetSection("FeatureManagement"));

        return services;
    }
}

