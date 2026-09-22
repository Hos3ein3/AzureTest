using System.Data.Common;
using System.Threading.Tasks;
using AzureTest.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;

namespace AzureTest.IntegrationTest;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("AzureTestDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:6.0")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString() },
                { "ConnectionStrings:MongoDb", _mongoContainer.GetConnectionString() },
                { "MongoDb:DatabaseName", "AzureTestDb_IntegrationTest" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing AppDbContext setup
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AzureTestDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove existing DbConnection setup
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // Add AppDbContext pointing to Testcontainer
            services.AddDbContext<AzureTestDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                // Interceptors are handled by standard DependencyInjection.cs 
                // but if we need to explicitly register them here, we can.
            });

            // Remove existing IMongoClient setup
            var mongoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMongoClient));

            if (mongoDescriptor != null)
            {
                services.Remove(mongoDescriptor);
            }

            services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoContainer.GetConnectionString()));
            
            // Note: IMongoDatabase registration is scoped and depends on IMongoClient. 
            // It will pick up the new IMongoClient automatically.
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _mongoContainer.StartAsync();

        // Ensure relational database is created/migrated
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync().AsTask();
        await _mongoContainer.DisposeAsync().AsTask();
    }
}
