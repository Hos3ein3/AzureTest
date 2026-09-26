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

    public NoSqlUnitOfWork(IMongoClient mongoClient, IMongoDatabase database)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public IResumeFilesRepository ResumeFiles => _resumeFiles ??= new ResumeFilesRepository(_database);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_database);
    public IResumeStatusLogRepository ResumeStatusLogs => _resumeStatusLogs ??= new ResumeStatusLogRepository(_database);

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
