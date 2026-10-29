

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
        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<ReadOnlyUserInterceptor>();
        services.AddScoped<ImpersonationReadOnlyInterceptor>();
        services.AddScoped<SingleRecordSettingInterceptor>();
        services.AddScoped<ConcurrencyStampInterceptor>();

        // ── PostgreSQL / EF Core ──────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var readOnlyConnectionString = configuration.GetConnectionString("ReadOnlyConnection") ?? connectionString;

        services.AddDbContext<AzureTestDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null));
            // Interceptors are injected via the service provider
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<DomainEventsInterceptor>(),
                sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<ReadOnlyUserInterceptor>(),
                sp.GetRequiredService<ImpersonationReadOnlyInterceptor>(),
                sp.GetRequiredService<SingleRecordSettingInterceptor>(),
                sp.GetRequiredService<ConcurrencyStampInterceptor>());
        });

        // ── Read-Only DbContext for CQRS Read Scale-Out ───────────────────────
        services.AddDbContext<ReadOnlyAzureTestDbContext>(options =>
        {
            options.UseNpgsql(readOnlyConnectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // ── SQL Repositories & Unit of Work ───────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReadOnlyFootballFantasyUnitOfWork, ReadOnlyFootballFantasyUnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IHabitTrackerRepository, HabitTrackerRepository>();
        services.AddScoped<IHabitLogRepository, HabitLogRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketMessageRepository, TicketMessageRepository>();
        services.AddScoped<IDebtRepository, DebtRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IPomodoroRepository, PomodoroRepository>();

        // ── Crypto Repositories ───────────────────────────────────────────────
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoAccountRepository, Repositories.Crypto.CryptoAccountRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoOrderRepository, Repositories.Crypto.CryptoOrderRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoPositionRepository, Repositories.Crypto.CryptoPositionRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoStrategyRepository, Repositories.Crypto.CryptoStrategyRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoBacktestRepository, Repositories.Crypto.CryptoBacktestRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoAlertRepository, Repositories.Crypto.CryptoAlertRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoJournalEntryRepository, Repositories.Crypto.CryptoJournalEntryRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Crypto.ICryptoApiSettingRepository, Repositories.Crypto.CryptoApiSettingRepository>();

        // ── Forex Repositories ────────────────────────────────────────────────
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexAccountRepository, Repositories.Forex.ForexAccountRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexOrderRepository, Repositories.Forex.ForexOrderRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexTradeRepository, Repositories.Forex.ForexTradeRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexStrategyRepository, Repositories.Forex.ForexStrategyRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexBacktestRepository, Repositories.Forex.ForexBacktestRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexEconomicAlertRepository, Repositories.Forex.ForexEconomicAlertRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexJournalEntryRepository, Repositories.Forex.ForexJournalEntryRepository>();
        services.AddScoped<AzureTest.Application.Interfaces.Data.Forex.IForexApiSettingRepository, Repositories.Forex.ForexApiSettingRepository>();

        // ── Factories ─────────────────────────────────────────────────────────
        services.AddScoped<IDatabaseFactory, DatabaseFactory>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        // ── MongoDB / NoSQL ───────────────────────────────────────────────────
        var mongoConnectionString = configuration.GetConnectionString("MongoDb");

        if (!string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            services.AddSingleton<IMongoClient>(sp => 
            {
                return new MongoClient(mongoConnectionString);
            });
            services.AddScoped(sp =>
            {
                var dbName = configuration.GetSection("MongoDb")["DatabaseName"];
                return sp.GetRequiredService<IMongoClient>().GetDatabase(dbName);
            });

            services.AddScoped<IResumeFilesRepository, ResumeFilesRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IResumeStatusLogRepository, ResumeStatusLogRepository>();
            services.AddScoped<IDebtPersonRepository, DebtPersonRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.AddScoped<ITrainingRepository, TrainingRepository>();
            services.AddScoped<ITrainingCourseRepository, TrainingCourseRepository>();
            services.AddScoped<IUserGymCourseRepository, UserGymCourseRepository>();
            services.AddScoped<IUserTrainingSessionRepository, UserTrainingSessionRepository>();
            services.AddScoped<IUserWeightLogRepository, UserWeightLogRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IWatchItemRepository, WatchItemRepository>();
            services.AddScoped<IFollowedPersonRepository, FollowedPersonRepository>();
            services.AddScoped<ICustomMediaListRepository, CustomMediaListRepository>();
            services.AddScoped<INoSqlUnitOfWork, NoSqlUnitOfWork>();
            services.AddScoped<AzureTest.Persistence.Seeding.CompanyDataSeeder>();
        }
        else
        {
            // MongoDB not configured — register no-op implementations so the app starts cleanly.
            // File metadata will not be persisted until a MongoDb connection string is provided.
            services.AddScoped<IResumeFilesRepository, NullResumeFilesRepository>();
            services.AddScoped<IResumeStatusLogRepository, NullResumeStatusLogRepository>();
            services.AddScoped<IDebtPersonRepository, NullDebtPersonRepository>();
            services.AddScoped<IExpenseCategoryRepository, NullExpenseCategoryRepository>();
            services.AddScoped<IUserPreferenceRepository, NullUserPreferenceRepository>();
            services.AddScoped<IAuditLogRepository, NullAuditLogRepository>();
            services.AddScoped<IWatchItemRepository, NullWatchItemRepository>();
            services.AddScoped<IFollowedPersonRepository, NullFollowedPersonRepository>();
            services.AddScoped<ICustomMediaListRepository, NullCustomMediaListRepository>();
        }

        return services;
    }
}
