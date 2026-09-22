using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace AzureTest.e2eTest;

public class E2ETestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("AzureTestE2EDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:6.0")
        .Build();

    public string ServerAddress { get; private set; } = "";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Intercept host creation to force Kestrel to listen on a real port.
        // We use port 0 so the OS assigns a free dynamic port.
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel(options =>
            {
                options.ListenLocalhost(0);
            });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString() },
                { "ConnectionStrings:MongoDb", _mongoContainer.GetConnectionString() },
                { "ConnectionStrings:Redis", "" }, // Disable real redis to avoid connection errors
                { "MongoDb:DatabaseName", "AzureTestE2EDb" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing AppDbContext setup
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AzureTestDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add AppDbContext pointing to Testcontainer
            services.AddDbContext<AzureTestDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Remove existing IMongoClient setup
            var mongoDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            if (mongoDescriptor != null) services.Remove(mongoDescriptor);

            services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoContainer.GetConnectionString()));

            // Register dummy cache service since Redis is disabled
            services.AddSingleton<AzureTest.Application.Interfaces.Services.ICompanyCacheService, DummyCompanyCacheService>();

            // Remove CompanyDataSeeder to avoid scraping live websites during CI integration tests
            var seederDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AzureTest.Persistence.Seeding.CompanyDataSeeder));
            if (seederDescriptor != null)
            {
                services.Remove(seederDescriptor);
            }
        });
    }

    private class DummyCompanyCacheService : AzureTest.Application.Interfaces.Services.ICompanyCacheService
    {
        public Task<System.Collections.Generic.IEnumerable<AzureTest.Contracts.DTOs.Companies.CompanyDto>?> GetCompaniesAsync(System.Threading.CancellationToken cancellationToken = default) 
            => Task.FromResult<System.Collections.Generic.IEnumerable<AzureTest.Contracts.DTOs.Companies.CompanyDto>?>(null);

        public Task InvalidateCompaniesAsync(System.Threading.CancellationToken cancellationToken = default) 
            => Task.CompletedTask;

        public Task SetCompaniesAsync(System.Collections.Generic.IEnumerable<AzureTest.Contracts.DTOs.Companies.CompanyDto> companies, System.Threading.CancellationToken cancellationToken = default) 
            => Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _mongoContainer.StartAsync();

        // Trigger host creation and startup so Kestrel binds to the port
        var client = this.CreateClient();

        // Extract the dynamically assigned URL
        var server = this.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        ServerAddress = addresses?.Addresses.FirstOrDefault() ?? "http://127.0.0.1";

        // Database migrations
        using var scope = this.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync().AsTask();
        await _mongoContainer.DisposeAsync().AsTask();
        await base.DisposeAsync();
    }
}
