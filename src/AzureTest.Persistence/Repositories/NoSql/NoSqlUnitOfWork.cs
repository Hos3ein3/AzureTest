using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NoSqlUnitOfWork : INoSqlUnitOfWork
{
    private readonly IMongoClient _mongoClient;
    private IClientSessionHandle? _session;

    public NoSqlUnitOfWork(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
    }

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
