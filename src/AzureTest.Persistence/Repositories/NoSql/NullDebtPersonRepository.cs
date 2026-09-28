using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullDebtPersonRepository : IDebtPersonRepository
{
    public Task<DebtPerson?> GetByIdAsync(Guid id) => Task.FromResult<DebtPerson?>(null);

    public Task<IReadOnlyList<DebtPerson>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<DebtPerson>>(Array.Empty<DebtPerson>());

    public Task<IReadOnlyList<DebtPerson>> GetAllAvailableAsync() =>
        Task.FromResult<IReadOnlyList<DebtPerson>>(Array.Empty<DebtPerson>());

    public Task<IReadOnlyList<DebtPerson>> FindAsync(Expression<Func<DebtPerson, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<DebtPerson>>(Array.Empty<DebtPerson>());

    public Task AddAsync(DebtPerson document) => Task.CompletedTask;

    public Task UpdateAsync(Guid id, DebtPerson document) => Task.CompletedTask;

    public Task RemoveAsync(Guid id) => Task.CompletedTask;

    public Task<IReadOnlyList<DebtPerson>> SearchByNameAsync(string term) =>
        Task.FromResult<IReadOnlyList<DebtPerson>>(Array.Empty<DebtPerson>());
}
