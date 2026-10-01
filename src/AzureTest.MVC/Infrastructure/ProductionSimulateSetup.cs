using AzureTest.Application.Abstractions;


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

        // ── Resolve the Docker socket for macOS Docker Desktop compatibility ─────
        // Active context is 'desktop-linux' => ~/.docker/run/docker.sock — probe FIRST.
        // /var/run/docker.sock also exists as a symlink but may point to the wrong daemon.
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidateSockets = new[]
        {
            System.IO.Path.Combine(home, ".docker", "run", "docker.sock"),      // macOS Docker Desktop (desktop-linux ctx)
            System.IO.Path.Combine(home, ".docker", "desktop", "docker.sock"),  // older macOS Docker Desktop
            "/var/run/docker.sock",                                              // Linux / symlink fallback
        };

        string? socketPath = null;
        foreach (var candidate in candidateSockets)
        {
            if (System.IO.File.Exists(candidate))
            {
                socketPath = candidate;
                break;
            }
        }

        if (socketPath == null)
            throw new InvalidOperationException(
                "❌ ProductionSimulate: Docker socket not found. " +
                "Make sure Docker Desktop is running. Checked:\n  " +
                string.Join("\n  ", candidateSockets));

        var dockerEndpoint = $"unix://{socketPath}";
        logger.LogInformation("🔌 Docker endpoint: {Endpoint}", dockerEndpoint);
        Environment.SetEnvironmentVariable("DOCKER_HOST", dockerEndpoint);

        // ── Build containers ──────────────────────────────────────────────────────
        // WithDockerEndpoint() passes the socket directly — no reliance on env-var timing.
        // WithCleanUp(false) disables the Ryuk reaper container (the actual source of the
        // hang on macOS). Cleanup is handled manually in ApplicationStopped below.
        _postgres = new PostgreSqlBuilder()
            .WithDockerEndpoint(dockerEndpoint)
            .WithImage("postgres:16-alpine")
            .WithDatabase("azuretest_sim")
            .WithUsername("sim_user")
            .WithPassword("sim_pass")
            .WithCleanUp(false)
            .Build();

        _mongo = new MongoDbBuilder()
            .WithDockerEndpoint(dockerEndpoint)
            .WithImage("mongo:7.0")
            .WithCleanUp(false)
            .Build();

        _redis = new RedisBuilder()
            .WithDockerEndpoint(dockerEndpoint)
            .WithImage("redis:7.2-alpine")
            .WithCleanUp(false)
            .Build();

        // Start all three in parallel — with a generous timeout so we fail fast with a clear error
        logger.LogInformation("⏳ Waiting for containers to start (timeout: 3 minutes)…");
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try
        {
            await Task.WhenAll(
                _postgres.StartAsync(cts.Token),
                _mongo.StartAsync(cts.Token),
                _redis.StartAsync(cts.Token)
            );
        }
        catch (Exception ex)
        {
            // Log the FULL exception so we can see what actually failed
            logger.LogError(ex,
                "❌ ProductionSimulate: Container startup failed. " +
                "Docker endpoint: {Endpoint}. " +
                "Exception type: {ExType}. " +
                "Message: {Message}",
                dockerEndpoint, ex.GetType().Name, ex.Message);

            if (ex is OperationCanceledException)
                throw new InvalidOperationException(
                    $"❌ ProductionSimulate: Containers failed to start within 3 minutes. " +
                    $"Docker endpoint: {dockerEndpoint}", ex);

            throw;  // re-throw original for all other errors
        }


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
