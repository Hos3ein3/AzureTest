using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AzureTest.MVC.Infrastructure;

/// <summary>
/// Provides the <c>ProductionSimulateLocal</c> environment setup.
///
/// Unlike <see cref="ProductionSimulateSetup"/> (which spins up ephemeral Testcontainers),
/// this environment connects to REAL local services defined in
/// <c>appsettings.ProductionSimulateLocal.json</c> (your already-running Docker Compose stack).
///
/// On startup it:
///   1. Applies any pending EF Core migrations.
///   2. Runs <see cref="FakeDataGenerator"/> ONLY if the database appears empty
///      (no roles exist yet), so you can restart safely without duplicating data.
///
/// On shutdown it:
///   - Drops the PostgreSQL database (azuretest_local_sim)
///   - Drops the MongoDB database
///   - Flushes all Redis keys
///
/// Usage in Program.cs:
/// <code>
///   if (app.Environment.IsEnvironment("ProductionSimulateLocal"))
///       await ProductionSimulateLocalSetup.SeedIfEmptyAsync(app);
/// </code>
/// </summary>
public static class ProductionSimulateLocalSetup
{
    /// <summary>
    /// Applies EF Core migrations, seeds fake data if the database is empty,
    /// and registers a shutdown hook that wipes all three data stores.
    /// Safe to call on every startup — seeding is idempotent.
    /// </summary>
    public static async Task SeedIfEmptyAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger   = services.GetRequiredService<ILogger<WebApplication>>();

        logger.LogInformation("🏠 ProductionSimulateLocal: Starting local simulation setup…");

        try
        {
            // ── 1. Apply pending EF Core migrations ──────────────────────────
            var db      = services.GetRequiredService<AzureTestDbContext>();
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation("📦 ProductionSimulateLocal: Applying {Count} pending migration(s)…", pending.Count());
                await db.Database.MigrateAsync();
                logger.LogInformation("✅ Migrations applied.");
            }
            else
            {
                logger.LogInformation("✅ No pending migrations.");
            }

            // ── 2. Check if database is already seeded ────────────────────────
            var roleManager    = services.GetRequiredService<RoleManager<ApplicationRole>>();
            bool alreadySeeded = await roleManager.Roles.AnyAsync();

            if (alreadySeeded)
            {
                logger.LogInformation("ℹ️  ProductionSimulateLocal: Data already exists — skipping FakeDataGenerator.");
            }
            else
            {
                // ── 3. Seed fake data ─────────────────────────────────────────
                logger.LogInformation("🌱 ProductionSimulateLocal: Database is empty. Seeding fake data with Bogus…");

                var noSql       = services.GetRequiredService<INoSqlUnitOfWork>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                await FakeDataGenerator.SeedAsync(db, noSql, userManager, roleManager);

                logger.LogInformation(
                    "✅ ProductionSimulateLocal: Seeding complete.\n" +
                    "   👤 user@azuretest.local   / 123456  (role: User)\n" +
                    "   🔑 admin@azuretest.local  / 123456  (role: Admin)\n" +
                    "   ⚙️  system@azuretest.local / 123456  (role: System)");
            }

            // ── 4. Register cleanup hook ──────────────────────────────────────
            // Capture connection info NOW while the scope is still alive.
            // We use ApplicationStopping (not ApplicationStopped) so that the
            // service provider is still accessible during cleanup.
            RegisterCleanupHook(app, logger, db.Database.GetConnectionString()!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "❌ ProductionSimulateLocal: Setup failed. " +
                "Check that PostgreSQL, MongoDB and Redis are running and " +
                "the connection strings in appsettings.ProductionSimulateLocal.json are correct. " +
                "Error: {Message}", ex.Message);
            throw;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Cleanup — registered once, fires on ApplicationStopping
    // ──────────────────────────────────────────────────────────────────────────

    private static void RegisterCleanupHook(WebApplication app, ILogger logger, string pgConnectionString)
    {
        // Read Mongo + Redis connection strings from configuration
        var cfg              = app.Services.GetRequiredService<IConfiguration>();
        
        var dropDatabases = cfg.GetValue<bool>("DropDatabasesOnShutdown");
        if (!dropDatabases)
        {
            logger.LogInformation("ℹ️  ProductionSimulateLocal: 'DropDatabasesOnShutdown' is false. Databases will NOT be dropped on shutdown.");
            return;
        }

        var mongoConnStr     = cfg.GetConnectionString("MongoDb")
                              ?? cfg["DatabaseSettings:MongoDb:ConnectionString"]
                              ?? "mongodb://localhost:27017";
        var mongoDbName      = ExtractMongoDatabaseName(mongoConnStr, cfg);
        var redisConnStr     = cfg.GetConnectionString("Redis")
                              ?? cfg["Redis:Configuration"]
                              ?? "localhost:6379";

        logger.LogInformation(
            "🗑️  ProductionSimulateLocal: Cleanup hook registered ('DropDatabasesOnShutdown' is true).\n" +
            "    Will DROP PostgreSQL DB, MongoDB DB '{MongoDb}', and FLUSH Redis on shutdown.",
            mongoDbName);


        // ApplicationStopping fires while the host is still running
        // (before DisposeAsync), so we can still create service scopes.
        app.Lifetime.ApplicationStopping.Register(() =>
        {
            // Run the async cleanup synchronously inside the sync Register callback
            CleanupAsync(app, logger, pgConnectionString, mongoConnStr, mongoDbName, redisConnStr)
                .GetAwaiter()
                .GetResult();
        });
    }

    private static async Task CleanupAsync(
        WebApplication app,
        ILogger logger,
        string pgConnectionString,
        string mongoConnStr,
        string mongoDbName,
        string redisConnStr)
    {
        logger.LogInformation("🗑️  ProductionSimulateLocal: Application is stopping — cleaning up data stores…");

        // ── PostgreSQL: drop the entire database ─────────────────────────────
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();
            logger.LogInformation("🗑️  Dropping PostgreSQL database…");
            await db.Database.EnsureDeletedAsync();
            logger.LogInformation("✅ PostgreSQL database dropped.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "⚠️  Failed to drop PostgreSQL database: {Message}", ex.Message);
        }

        // ── MongoDB: drop the entire database ────────────────────────────────
        try
        {
            logger.LogInformation("🗑️  Dropping MongoDB database '{DbName}'…", mongoDbName);
            var mongoClient = new MongoClient(mongoConnStr);
            await mongoClient.DropDatabaseAsync(mongoDbName);
            logger.LogInformation("✅ MongoDB database '{DbName}' dropped.", mongoDbName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "⚠️  Failed to drop MongoDB database: {Message}", ex.Message);
        }

        // ── Redis: flush all keys ─────────────────────────────────────────────
        try
        {
            logger.LogInformation("🗑️  Flushing Redis (FLUSHALL)…");

            // Parse the connection string to a proper format if needed
            // (Bogus generates "host:port" which StackExchange.Redis accepts directly)
            using var redis = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(
                redisConnStr + ",allowAdmin=true");

            foreach (var endpoint in redis.GetEndPoints())
            {
                var server = redis.GetServer(endpoint);
                if (server.IsConnected)
                    await server.FlushAllDatabasesAsync();
            }
            logger.LogInformation("✅ Redis flushed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "⚠️  Failed to flush Redis: {Message}", ex.Message);
        }

        logger.LogInformation("✅ ProductionSimulateLocal: All data stores cleaned up.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helper: extract database name from a MongoDB connection string
    // "mongodb://localhost:27017/MyDatabase"  →  "MyDatabase"
    // "mongodb://localhost:27017"             →  falls back to config / default
    // ──────────────────────────────────────────────────────────────────────────
    private static string ExtractMongoDatabaseName(string connStr, IConfiguration cfg)
    {
        try
        {
            var url = new MongoDB.Driver.MongoUrl(connStr);
            if (!string.IsNullOrWhiteSpace(url.DatabaseName))
                return url.DatabaseName;
        }
        catch { /* ignored */ }

        // Fallback: explicit setting in config
        return cfg["DatabaseSettings:MongoDb:DatabaseName"] ?? "AzureTestDb.LocalSim";
    }
}
