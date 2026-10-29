using System.Data.Common;
using System.Threading.Tasks;
using AzureTest.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
                { "ConnectionStrings:Redis", "" }, // Disable real redis to avoid connection errors
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

            // Register dummy cache service since Redis is disabled
            services.AddSingleton<AzureTest.Application.Interfaces.Services.ICompanyCacheService, DummyCompanyCacheService>();

            // Remove CompanyDataSeeder to avoid scraping live websites during CI integration tests
            var seederDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(AzureTest.Persistence.Seeding.CompanyDataSeeder));
            if (seederDescriptor != null)
            {
                services.Remove(seederDescriptor);
            }
            // Register stub ICurrentUser
            services.AddScoped<AzureTest.Application.Interfaces.Services.ICurrentUser, TestCurrentUser>();
            
            // Register stub INotificationService
            services.AddScoped<AzureTest.Application.Interfaces.Services.INotificationService, DummyNotificationService>();
        });
    }

    private class TestCurrentUser : AzureTest.Application.Interfaces.Services.ICurrentUser
    {
        public static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public Guid Id => UserId;
        public Guid TenantId => UserId;
        public string Email => "test@test.com";
        public string? PhoneNumber => null;
        public string? ProfileImage => null;
        public string FirstName => "Test";
        public string LastName => "User";
        public string FullName => "Test User";
        public System.Collections.Generic.IReadOnlyList<string> Roles => new System.Collections.Generic.List<string> { "User" };
        public System.Collections.Generic.IReadOnlyList<string> DirectPermissions => new System.Collections.Generic.List<string>();
        public System.Collections.Generic.IReadOnlyList<string> RolePermissions => new System.Collections.Generic.List<string>();
        public System.Collections.Generic.IReadOnlyList<string> Permissions => new System.Collections.Generic.List<string>();
        public bool IsAdmin => false;
        public bool IsReadOnly => false;
        public bool HasAccess() => true;
        public bool HasAccess(string url) => true;
        public bool HasAccess(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, string? url = null) => true;
        public bool HasDirectPermission(string permissionKey) => true;
        public bool HasPermission(string permissionKey) => true;
        public bool BelongToCurrentUser(string entityUserId) => true;
        public bool BelongToCurrentUser(Guid entityUserId) => true;
    }


    private class DummyNotificationService : AzureTest.Application.Interfaces.Services.INotificationService
    {
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
