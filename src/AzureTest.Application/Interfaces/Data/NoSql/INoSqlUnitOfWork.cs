using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface INoSqlUnitOfWork : IDisposable
{
    IResumeFilesRepository ResumeFiles { get; }
    ICompanyRepository Companies { get; }
    IResumeStatusLogRepository ResumeStatusLogs { get; }
    IDebtPersonRepository DebtPersons { get; }
    IExpenseCategoryRepository ExpenseCategories { get; }
    IUserPreferenceRepository UserPreferences { get; }

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
