using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace AzureTest.MVC.Infrastructure;

/// <summary>
/// Provides the <c>ProductionSimulate</c> environment setup:
/// spins up ephemeral Docker containers for PostgreSQL, MongoDB and Redis,
/// overrides configuration connection strings, applies EF Core migrations,
/// and seeds the database with rich Bogus fake data.
///
/// Usage in Program.cs (single check, no other changes required):
/// <code>
///     if (builder.Environment.IsEnvironment("ProductionSimulate"))
///         await ProductionSimulateSetup.ConfigureAsync(builder);
/// </code>
/// After <c>app = builder.Build()</c>:
/// <code>
///     if (app.Environment.IsEnvironment("ProductionSimulate"))
///         await ProductionSimulateSetup.SeedAsync(app);
/// </code>
/// Containers are disposed automatically via <see cref="IApplicationLifetime"/> when the app stops.
/// </summary>
public static class ProductionSimulateSetup
{
    // Held as statics so the lifetime hook can dispose them after app.Run()
    private static PostgreSqlContainer? _postgres;
    private static MongoDbContainer? _mongo;
    private static RedisContainer? _redis;

    /// <summary>
    /// Starts all three Testcontainers and overrides <paramref name="builder"/>'s
    /// configuration with their connection strings — must be called BEFORE
    /// <c>builder.Build()</c> so that EF Core, MongoDB driver and Redis pick them up.
    /// </summary>
    public static async Task ConfigureAsync(WebApplicationBuilder builder)
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("ProductionSimulate");

        logger.LogInformation("🐳 ProductionSimulate: Starting ephemeral containers…");

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("azuretest_sim")
            .WithUsername("sim_user")
            .WithPassword("sim_pass")
            .Build();

        _mongo = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();

        _redis = new RedisBuilder()
            .WithImage("redis:7.2-alpine")
            .Build();

        // Start all three in parallel for faster startup
        await Task.WhenAll(
            _postgres.StartAsync(),
            _mongo.StartAsync(),
            _redis.StartAsync()
        );

        logger.LogInformation("✅ ProductionSimulate: All containers running.");

        // Override connection strings so DI picks up the container endpoints
        builder.Configuration["ConnectionStrings:PostgreSQL"] = _postgres.GetConnectionString();
        builder.Configuration["DatabaseSettings:MongoDb:ConnectionString"] = _mongo.GetConnectionString();
        builder.Configuration["Redis:Configuration"] = _redis.GetConnectionString();

        logger.LogInformation("🔗 Postgres  → {Conn}", _postgres.GetConnectionString());
        logger.LogInformation("🔗 MongoDB   → {Conn}", _mongo.GetConnectionString());
        logger.LogInformation("🔗 Redis     → {Conn}", _redis.GetConnectionString());
    }

    /// <summary>
    /// Applies pending EF Core migrations and seeds fake data via <see cref="FakeDataGenerator"/>.
    /// Must be called AFTER <c>app = builder.Build()</c>.
    /// </summary>
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            // Apply EF Core migrations to the ephemeral Postgres container
            var db = services.GetRequiredService<AzureTestDbContext>();
            logger.LogInformation("📦 ProductionSimulate: Applying EF Core migrations…");
            await db.Database.MigrateAsync();
            logger.LogInformation("✅ Migrations applied.");

            // Seed fake data
            var noSql = services.GetRequiredService<INoSqlUnitOfWork>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            logger.LogInformation("🌱 ProductionSimulate: Seeding fake data with Bogus…");
            await FakeDataGenerator.SeedAsync(db, noSql, userManager, roleManager);
            logger.LogInformation("✅ Fake data seeded. Admin: admin@simulate.com / Admin123!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ ProductionSimulate: Error during seeding.");
            throw;
        }

        // Register cleanup hook — containers are destroyed on app shutdown
        app.Lifetime.ApplicationStopped.Register(() =>
        {
            app.Logger.LogInformation("🗑️ ProductionSimulate: Disposing ephemeral containers…");
            _postgres?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _mongo?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _redis?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            app.Logger.LogInformation("✅ Containers disposed.");
        });
    }
}
