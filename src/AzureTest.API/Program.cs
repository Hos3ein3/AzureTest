using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using AzureTest.API.Hubs;
using AzureTest.API.Feature;
using AzureTest.Application;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Seeding;
using AzureTest.Persistence;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Seeding;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Read Feature Flags & Options ──────────────────────────────────────────
var infraOptions = new InfrastructureFeatureOptions();
builder.Configuration.GetSection("FeatureManagement").Bind(infraOptions);

// ── 2. Register Application, Infrastructure & Persistence Layers ─────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(infraOptions, builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// ── 3. Cross-Cutting Services (Feature Management, Caching, Data Protection) ─
builder.Services.AddFeatureManagement();
builder.Services.AddProblemDetails();

#pragma warning disable EXTEXP0018 // HybridCache is preview in .NET 9
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddDataProtection()
    .SetApplicationName("AzureTest")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// ── 4. API Versioning & OpenAPI / Scalar ─────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationHubDispatcher, NotificationHubDispatcher>();

// ── 5. CORS Configuration ────────────────────────────────────────────────────
const string CorsPolicy = "AppCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? [
        "http://localhost:4200",
        "http://localhost:3000",
        "http://localhost:5173",
        "http://localhost:5000",
        "https://localhost:5001",
        "http://localhost:5191",
        "https://localhost:7191",
        "http://localhost:5143",
        "https://localhost:7143"
    ];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Pagination", "Content-Disposition", "X-Refresh-Token");
        }
        else
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithExposedHeaders("X-Pagination", "Content-Disposition", "X-Refresh-Token");
            }
            else
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod();
            }
        }
    });

    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Pagination", "Content-Disposition", "X-Refresh-Token");
        }
        else
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithExposedHeaders("X-Pagination", "Content-Disposition", "X-Refresh-Token");
            }
            else
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod();
            }
        }
    });
});
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = _ => true;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();

        // 1. Bearer JWT UserToken
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT UserToken / Access Token."
        };

        // 2. RefreshToken (API Key Header)
        document.Components.SecuritySchemes["RefreshToken"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
            Name = "X-Refresh-Token",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your Refresh Token for JWT session renewal."
        };

        // 3. Security requirements
        document.Security ??= new List<Microsoft.OpenApi.OpenApiSecurityRequirement>();
        var req = new Microsoft.OpenApi.OpenApiSecurityRequirement();
        req.Add(new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document), new List<string>());
        req.Add(new Microsoft.OpenApi.OpenApiSecuritySchemeReference("RefreshToken", document), new List<string>());
        document.Security.Add(req);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// ── Check & Log Sensitive Secrets Presence (Never Log Values) ────────────────
LogSecretConfigurationsPresence(app);

// ── 5. Cold Start Migrations & Seeders ────────────────────────────────────────
var isSimulateEnv = app.Environment.IsEnvironment("ProductionSimulate") ||
                    app.Environment.IsEnvironment("ProductionSimulateLocal");
if (!isSimulateEnv)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        await MigrateWithRetryAsync(app.Services, logger);

        var companySeeder = scope.ServiceProvider.GetService<CompanyDataSeeder>();
        if (companySeeder != null)
        {
            await companySeeder.SeedIfEmptyAsync();
        }

        var identitySeeder = scope.ServiceProvider.GetService<IdentityDataSeeder>();
        if (identitySeeder != null)
        {
            await identitySeeder.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations and seeders.");
        throw;
    }
}

app.Logger.LogInformation("Environment: {EnvironmentName}", app.Environment.EnvironmentName);

// ── 6. HTTP Request Pipeline Configuration ───────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// ── 7. Scalar & OpenAPI Reference ────────────────────────────────────────────
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("AzureTest API Reference")
           .WithPreferredScheme("Bearer")
           .WithHttpBearerAuthentication(bearer =>
           {
               bearer.Token = "";
           })
           .WithApiKeyAuthentication(apiKey =>
           {
               apiKey.Token = "";
           });
});

// Default route redirects to Scalar
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
app.MapGet("/scalar", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

// ── 8. Minimal API Routes (VSA style) & SignalR Hubs ────────────────────────
app.MapApiRoutes();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<NotificationHub>("/hubs/notifications");

// ── 9. Health Check & Metrics Endpoints ─────────────────────────────────────
if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHealthCheck"))
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();
}

app.MapPrometheusScrapingEndpoint().AllowAnonymous();

app.Run();

// ── Helper Methods ───────────────────────────────────────────────────────────
static async Task MigrateWithRetryAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();

            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
                await db.Database.MigrateAsync();

            return;
        }
        catch (Exception ex) when (attempt < maxAttempts &&
                                   (ex is NpgsqlException || ex is TimeoutException ||
                                    ex.InnerException is System.Net.Sockets.SocketException))
        {
            logger.LogWarning(ex,
                "Database not ready (attempt {Attempt}/{Max}). Retrying in {Delay}s...",
                attempt, maxAttempts, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

static void LogSecretConfigurationsPresence(WebApplication app)
{
    var secretsToCheck = new (string EnvVar, string ConfigKey)[]
    {
        ("Authentication__Google__ClientId", "Authentication:Google:ClientId"),
        ("Authentication__Google__ClientSecret", "Authentication:Google:ClientSecret"),
        ("EmailSettings__Username", "EmailSettings:Username"),
        ("EmailSettings__Password", "EmailSettings:Password")
    };

    foreach (var (envVar, configKey) in secretsToCheck)
    {
        var rawEnv = Environment.GetEnvironmentVariable(envVar);
        var configVal = app.Configuration[configKey] ?? app.Configuration[envVar];

        var effectiveVal = !string.IsNullOrWhiteSpace(rawEnv) ? rawEnv : configVal;
        var exists = !string.IsNullOrWhiteSpace(effectiveVal) &&
                     !(effectiveVal.StartsWith("${") && effectiveVal.EndsWith("}"));

        var status = exists ? "exist" : "not exist";

        app.Logger.LogInformation("Configuration secret check -> env:key {EnvVar} ({ConfigKey}): {Status}",
            envVar,
            configKey,
            status);
    }
}
