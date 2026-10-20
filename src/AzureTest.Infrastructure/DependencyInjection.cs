

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
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        // ── Crawler Service ───────────────────────────────────────────────────
        services.AddHttpClient<IIndCrawlerService, IndCrawlerService>();
        services.AddScoped<AzureTest.Application.Interfaces.Services.IHouseCrawlerService, AzureTest.Infrastructure.Services.HouseCrawlerService>();

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
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            });
        }
        
        services.AddScoped<IRedisService, RedisService>();

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
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantBackfillJob, AzureTest.Infrastructure.Jobs.TenantBackfillJob>();
        services.AddScoped<AzureTest.Application.Interfaces.Services.IValidationService, AzureTest.Infrastructure.Services.ValidationService>();
        services.AddSingleton<AzureTest.Application.Interfaces.Services.ISampleNoteStore, AzureTest.Infrastructure.Services.SampleNoteStore>();
        services.AddTransient<AzureTest.Infrastructure.Seeding.IdentityDataSeeder>();

        services.AddOptions<MongoDbSettings>()
            .Configure<IConfiguration>((options, config) => {
                config.GetSection("MongoDb").Bind(options);
                options.ConnectionString = config.GetConnectionString("MongoDb") ?? string.Empty;
            })
            .Validate(opts => {
                var res = new AzureTest.Infrastructure.Configuration.Validators.MongoDbSettingsValidator().Validate(opts);
                if(!res.IsValid) throw new System.Exception(string.Join(", ", res.Errors));
                return true;
            })
            .ValidateOnStart();

        services.AddOptions<AzureTest.Infrastructure.Configuration.JwtSettings>()
            .Bind(configuration.GetSection(AzureTest.Infrastructure.Configuration.JwtSettings.SectionName))
            .Validate(opts => {
                var res = new AzureTest.Infrastructure.Configuration.Validators.JwtSettingsValidator().Validate(opts);
                if(!res.IsValid) throw new System.Exception(string.Join(", ", res.Errors));
                return true;
            })
            .ValidateOnStart();

        services.AddOptions<AzureTest.Infrastructure.Configuration.IdentitySettings>()
            .Bind(configuration.GetSection(AzureTest.Infrastructure.Configuration.IdentitySettings.SectionName))
            .Validate(opts => {
                var res = new AzureTest.Infrastructure.Configuration.Validators.IdentitySettingsValidator().Validate(opts);
                if(!res.IsValid) throw new System.Exception(string.Join(", ", res.Errors));
                return true;
            })
            .ValidateOnStart();

        services.AddOptions<HealthCheckUISettings>()
            .Bind(configuration.GetSection("HealthChecksUI"))
            .Validate(opts => {
                var res = new HealthCheckUISettingsValidator().Validate(opts);
                if(!res.IsValid) throw new System.Exception(string.Join(", ", res.Errors));
                return true;
            })
            .ValidateOnStart();

        services.Configure<InfrastructureFeatureOptions>(configuration.GetSection("FeatureManagement"));

        services.AddOptions<DashboardSettings>()
            .BindConfiguration("Dashboard")
            .ValidateOnStart();

        services.AddSingleton<AzureTest.Application.Messaging.IEventDispatcher, AzureTest.Infrastructure.Services.EventDispatcher>();
        services.AddTransient<AzureTest.Application.Messaging.IEventHandler<AzureTest.Application.Permissions.RolePermissionsChangedEvent>, AzureTest.Application.Permissions.RolePermissionsChangedEventHandler>();
        services.AddTransient<AzureTest.Application.Messaging.IEventHandler<AzureTest.Application.Users.UserRegisteredEvent>, AzureTest.Application.Users.UserRegisteredEventHandler>();

        services.AddHttpClient<AzureTest.Application.Interfaces.Services.IFootballApiService, AzureTest.Infrastructure.Services.FootballApiService>();

        return services;
    }
}

