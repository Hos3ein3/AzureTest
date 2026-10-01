

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NoSqlUnitOfWork : INoSqlUnitOfWork
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;

    private IResumeFilesRepository? _resumeFiles;
    private ICompanyRepository? _companies;
    private IResumeStatusLogRepository? _resumeStatusLogs;
    private IDebtPersonRepository? _debtPersons;
    private IExpenseCategoryRepository? _expenseCategories;
    private IUserPreferenceRepository? _userPreferences;
    private IDashboardComponentRepository? _dashboardComponents;
    private IDashboardRawComponentRepository? _dashboardRawComponents;
    private IUserDashboardRepository? _userDashboards;
    private IJobSearchHistoryRepository? _jobSearchHistories;
    private IIdeaRepository? _ideas;



    public NoSqlUnitOfWork(IMongoClient mongoClient, IMongoDatabase database)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public IResumeFilesRepository ResumeFiles => _resumeFiles ??= new ResumeFilesRepository(_database);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_database);
    public IResumeStatusLogRepository ResumeStatusLogs => _resumeStatusLogs ??= new ResumeStatusLogRepository(_database);
    public IDebtPersonRepository DebtPersons => _debtPersons ??= new DebtPersonRepository(_database);
    public IExpenseCategoryRepository ExpenseCategories => _expenseCategories ??= new ExpenseCategoryRepository(_database);
    public IUserPreferenceRepository UserPreferences => _userPreferences ??= new UserPreferenceRepository(_database);
    public IDashboardComponentRepository DashboardComponents => _dashboardComponents ??= new DashboardComponentRepository(_database);
    public IDashboardRawComponentRepository DashboardRawComponents => _dashboardRawComponents ??= new DashboardRawComponentRepository(_database);
    public IUserDashboardRepository UserDashboards => _userDashboards ??= new UserDashboardRepository(_database);
    public IJobSearchHistoryRepository JobSearchHistories => _jobSearchHistories ??= new JobSearchHistoryRepository(_database);
    public IIdeaRepository Ideas => _ideas ??= new IdeaRepository(_database);



    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have a direct "SaveChanges" like EF Core unless using explicit transactions.
        // If a transaction session is active, commit it.
        if (_session != null && _session.IsInTransaction)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            return true;
        }

        // If no explicit transaction, modifications are already saved incrementally.
        return true;
    }

    public void Dispose()
    {
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}
