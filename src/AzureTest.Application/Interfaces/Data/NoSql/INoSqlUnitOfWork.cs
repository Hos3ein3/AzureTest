using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface INoSqlUnitOfWork : IDisposable
{
    IResumeFilesRepository ResumeFiles { get; }
    ICompanyRepository Companies { get; }
    IResumeStatusLogRepository ResumeStatusLogs { get; }

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
