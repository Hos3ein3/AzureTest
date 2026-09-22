using AzureTest.Application;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Persistence;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Read Infrastructure Feature Options
var infraOptions = new InfrastructureFeatureOptions();
builder.Configuration.GetSection("FeatureManagement").Bind(infraOptions);

// Add layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(infraOptions, builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// Feature Management
builder.Services.AddFeatureManagement();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Add HealthChecks UI with In-Memory Storage
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

builder.Services.AddExceptionHandler<AzureTest.MVC.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ── Auto-apply pending EF Core migrations on cold start ─────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
    {
        app.Logger.LogInformation("Applying {Count} pending migration(s)...", pending.Count());
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Migrations applied successfully.");
    }

    // ── Auto-seed IND Companies on cold start ────────────────────────────────
    var companySeeder = scope.ServiceProvider.GetService<AzureTest.Persistence.Seeding.CompanyDataSeeder>();
    if (companySeeder != null)
    {
        await companySeeder.SeedIfEmptyAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while applying migrations.");
    throw;
}

app.Logger.LogInformation(app.Environment.EnvironmentName, "Environment {0}");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecksUI(options => 
{
    options.UIPath = "/healthchecks-ui";
});
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapPrometheusScrapingEndpoint();
app.UseHangfireDashboard("/hangfire");

app.Run();