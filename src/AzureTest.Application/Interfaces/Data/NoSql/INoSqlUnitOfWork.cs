using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface INoSqlUnitOfWork : IDisposable
{
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
