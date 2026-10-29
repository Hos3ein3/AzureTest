using WebMarkupMin.AspNetCore8;
using Microsoft.AspNetCore.DataProtection;
using AzureTest.Application;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Seeding;
using AzureTest.MVC.Hubs;
using AzureTest.MVC.Infrastructure;
using AzureTest.Persistence;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Seeding;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Npgsql;

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

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates.
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddDataProtection()
    .SetApplicationName("AzureTest")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// ── 4. Register MVC Layer Services (Controllers, Views, Filters, HealthChecks UI, WebOptimizer) ─
builder.Services.AddMvcLayerServices(builder.Configuration, builder.Environment);

// ── 5. Production Simulation Testcontainer Bootstrap ─────────────────────────
if (builder.Environment.IsEnvironment("ProductionSimulate"))
{
    await ProductionSimulateSetup.ConfigureAsync(builder);
}

// ── 6. Normalization / Widget Partial Views Fix ──────────────────────────────
NormalizeWidgetPartialViews(builder.Environment.ContentRootPath);

var app = builder.Build();

// ── 7. Cold Start Migrations & Seeders ────────────────────────────────────────
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
        app.Logger.LogError(ex, "An error occurred while applying migrations.");
        throw;
    }
}

if (app.Environment.IsEnvironment("ProductionSimulate"))
{
    try
    {
        await ProductionSimulateSetup.SeedAsync(app);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred during ProductionSimulate seeding.");
        throw;
    }
}

if (app.Environment.IsEnvironment("ProductionSimulateLocal"))
{
    try
    {
        await ProductionSimulateLocalSetup.SeedIfEmptyAsync(app);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred during ProductionSimulateLocal seeding.");
        throw;
    }
}

// ── 8. NoSql Dashboard Components Deduplication ──────────────────────────────
await DeduplicateDashboardComponentsAsync(app);

app.Logger.LogInformation(app.Environment.EnvironmentName, "Environment {0}");

// ── 9. HTTP Request Pipeline Configuration ───────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseWebOptimizer();
    app.UseWebMarkupMin();
}

app.UseStaticFiles();
app.UseForwardedHeaders(); // must run before UseAuthentication
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// ── 10. SignalR Hub Endpoints ────────────────────────────────────────────────
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<FantasyAuctionHub>("/auctionHub");
app.MapHub<ShoppingListHub>("/shoppingListHub");

// ── 11. Controller Routes ────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "dashboard_dynamic",
    pattern: "{slug}",
    defaults: new { controller = "Dashboard", action = "ViewDashboard" },
    constraints: new { slug = new DashboardRouteConstraint() }
).WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

// ── 12. Health Check & Metrics Endpoints ─────────────────────────────────────
if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHealthCheck"))
{
    if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHealthCheckUI"))
    {
        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/healthchecks-ui";
        }).AllowAnonymous();
    }

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();
}

app.MapPrometheusScrapingEndpoint().AllowAnonymous();

// ── 13. Hangfire Dashboard & Recurring Job Registrations ─────────────────────
if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHangfire"))
{
    app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
    {
        Authorization = new[] { new HangfireAdminAuthorizationFilter() },
        DashboardTitle = "TrackerPro Background Jobs"
    });

    try
    {
        using var scope = app.Services.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetService<IRecurringJobManager>();
        recurringJobManager?.AddOrUpdate<ITenantBackfillJob>(
            "tenant-id-backfill-sync",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily(2));

        recurringJobManager?.AddOrUpdate<ITradingPriceSyncJob>(
            "paper-trading-price-sl-tp-monitor",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Minutely());
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Failed to register recurring Hangfire jobs.");
    }
}

app.Run();

// ── Helper Methods ───────────────────────────────────────────────────────────
static void NormalizeWidgetPartialViews(string contentRootPath)
{
    try
    {
        var widgetsDir = Path.Combine(contentRootPath, "Views", "Shared", "Widgets");
        var renames = new Dictionary<string, string>
        {
            { "_WidgetApplicationsByStatusChart.cshtml", "_WidgetApplicationByStatus.cshtml" },
            { "_WidgetApplicationsOverTimeChart.cshtml", "_WidgetApplicationOverTime.cshtml" },
            { "_WidgetApplicationSourcesChart.cshtml", "_WidgetApplicationSources.cshtml" },
            { "_WidgetDebtsByPersonChart.cshtml", "_WidgetDebtsByPerson.cshtml" },
            { "_WidgetExpensesByCategoryChart.cshtml", "_WidgetExpensesByCategory.cshtml" },
            { "_WidgetHabitAdherenceChart.cshtml", "_WidgetHabitAdherence.cshtml" },
            { "_WidgetMonthlyFinanceChart.cshtml", "_WidgetMonthlyCashFlow.cshtml" },
            { "_WidgetTicketStatusChart.cshtml", "_WidgetSupportTickets.cshtml" },
            { "_WidgetTopLocationsChart.cshtml", "_WidgetTopLocations.cshtml" },
            { "_WidgetTopPositionsChart.cshtml", "_WidgetTopPositions.cshtml" }
        };

        foreach (var r in renames)
        {
            var oldPath = Path.Combine(widgetsDir, r.Key);
            var newPath = Path.Combine(widgetsDir, r.Value);
            if (File.Exists(oldPath)) File.Move(oldPath, newPath, true);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to rename widgets: " + ex.Message);
    }
}

static async Task DeduplicateDashboardComponentsAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var noSql = scope.ServiceProvider.GetRequiredService<INoSqlUnitOfWork>();
        var components = await noSql.DashboardComponents.GetAllAsync();
        var duplicates = components.GroupBy(c => c.Name).Where(g => g.Count() > 1).ToList();
        foreach (var group in duplicates)
        {
            var toDelete = group.Skip(1).ToList();
            foreach (var item in toDelete)
            {
                await noSql.DashboardComponents.RemoveAsync(item.Id);
            }
        }
        if (duplicates.Any())
        {
            await noSql.SaveChangesAsync();
            app.Logger.LogInformation("Removed duplicate dashboard components.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to deduplicate components.");
    }
}

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
