using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Persistence.Configurations.NoSql;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Factories;
using AzureTest.Persistence.Interceptors;
using AzureTest.Persistence.Repositories;
using AzureTest.Persistence.Repositories.NoSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AzureTest.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── MongoDB BSON Class Maps ───────────────────────────────────────────
        // Must be called before any MongoDB type is serialized/deserialized
        MongoDbClassMaps.Register();

        // ── EF Core Interceptors ──────────────────────────────────────────────
        // Registered as Scoped so they share the same lifetime as the DbContext
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventsInterceptor>();

        // ── PostgreSQL / EF Core ──────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AzureTestDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);

            // Interceptors are injected via the service provider
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<DomainEventsInterceptor>());
        });

        // ── SQL Repositories & Unit of Work ───────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IResumeRepository, ResumeRepository>();

        // ── Factories ─────────────────────────────────────────────────────────
        services.AddScoped<IDatabaseFactory, DatabaseFactory>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        // ── MongoDB / NoSQL ───────────────────────────────────────────────────
        var mongoConnectionString = configuration.GetConnectionString("MongoDb");
        var mongoDatabaseName = configuration["MongoDb:DatabaseName"] ?? "AzureTestDb";

        if (!string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
            services.AddScoped(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

            services.AddScoped<IResumeFilesRepository, ResumeFilesRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IStatusLogRepository, StatusLogRepository>();
            services.AddScoped<INoSqlUnitOfWork, NoSqlUnitOfWork>();
            services.AddScoped<AzureTest.Persistence.Seeding.CompanyDataSeeder>();
        }
        else
        {
            // MongoDB not configured — register no-op implementations so the app starts cleanly.
            // File metadata will not be persisted until a MongoDb connection string is provided.
            services.AddScoped<IResumeFilesRepository, NullResumeFilesRepository>();
            services.AddScoped<IStatusLogRepository, NullStatusLogRepository>();
        }

        return services;
    }
}
