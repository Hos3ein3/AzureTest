using System;
using System.IO;
using System.Linq;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;
using AzureTest.MVC.Filters;
using AzureTest.MVC.Infrastructure;
using AzureTest.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WebMarkupMin.AspNetCore8;

namespace AzureTest.MVC.Infrastructure;

public static class MvcServiceRegistration
{
    public static IServiceCollection AddMvcLayerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // ── 1. MVC Controllers & Razor Views ──────────────────────────────────────
        services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
            options.Filters.Add(typeof(DynamicAuthorizationFilter));
        })
        .AddRazorRuntimeCompilation(options =>
        {
            var mvcDir = Path.Combine(environment.ContentRootPath, "src", "AzureTest.MVC");
            if (Directory.Exists(mvcDir))
            {
                options.FileProviders.Add(new PhysicalFileProvider(mvcDir));
            }

            var binViewsDir = Path.Combine(AppContext.BaseDirectory, "Views");
            if (Directory.Exists(binViewsDir))
            {
                options.FileProviders.Add(new PhysicalFileProvider(AppContext.BaseDirectory));
            }
        });

        // ── 2. MVC Scoped & Singleton Services / Filters ──────────────────────────
        services.AddScoped<IDashboardDataLoader, DashboardDataLoader>();
        services.AddScoped<IPermissionScanner, PermissionScanner>();
        services.AddScoped<DynamicAuthorizationFilter>();
        services.AddScoped<ImpersonationReadOnlyFilter>();

        // ── 3. Health Checks UI (In-Memory Storage) ───────────────────────────────
        if (configuration.GetValue<bool>("FeatureManagement:EnableHealthCheck") &&
            configuration.GetValue<bool>("FeatureManagement:EnableHealthCheckUI"))
        {
            var hcSettings = configuration.GetSection("HealthChecksUI").Get<HealthCheckUISettings>();
            services.AddHealthChecksUI(setup =>
            {
                var configuredEndpoints = configuration.GetSection("HealthChecksUI:HealthChecks").GetChildren();
                if (!configuredEndpoints.Any())
                {
                    var defaultUri = environment.IsDevelopment()
                        ? "http://127.0.0.1:5191/health"
                        : "http://127.0.0.1:8080/health";
                    setup.AddHealthCheckEndpoint("AzureTest Health", defaultUri);
                }

                if (hcSettings != null)
                {
                    setup.SetEvaluationTimeInSeconds(hcSettings.EvaluationTimeInSeconds);
                    setup.SetMinimumSecondsBetweenFailureNotifications(hcSettings.MinimumSecondsBetweenFailureNotifications);
                }
            }).AddInMemoryStorage();
        }

        // ── 4. Exception Handling, Problem Details & Realtime Hubs ─────────────────
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddSignalR();
        services.AddSingleton<IUserTracker, UserTracker>();

        // ── 5. Forwarded Headers ──────────────────────────────────────────────────
        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.KnownNetworks.Clear();
            o.KnownProxies.Clear();
        });

        // ── 6. HTML, CSS, JS Minification (Only in Production) ────────────────────
        if (environment.IsProduction())
        {
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.MinifyJsFiles("js/**/*.js");
                pipeline.MinifyCssFiles("css/**/*.css");
            });

            services.AddWebMarkupMin(options =>
            {
                options.AllowMinificationInDevelopmentEnvironment = false;
                options.AllowCompressionInDevelopmentEnvironment = false;
            })
            .AddHtmlMinification(options =>
            {
                options.MinificationSettings.RemoveRedundantAttributes = true;
                options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
                options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
                options.MinificationSettings.RemoveHtmlComments = true;
            })
            .AddHttpCompression();
        }

        return services;
    }
}
