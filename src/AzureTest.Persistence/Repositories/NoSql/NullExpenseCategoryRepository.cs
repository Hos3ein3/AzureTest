

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IExpenseCategoryRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullExpenseCategoryRepository : IExpenseCategoryRepository
{
    public Task<ExpenseCategory?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default) => Task.FromResult<ExpenseCategory?>(null);

    public Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>(Array.Empty<ExpenseCategory>());

    public Task<IReadOnlyList<ExpenseCategory>> GetAllAvailableAsync() =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>(Array.Empty<ExpenseCategory>());

    public Task<IReadOnlyList<ExpenseCategory>> FindAsync(Expression<Func<ExpenseCategory, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>(Array.Empty<ExpenseCategory>());

    public Task AddAsync(ExpenseCategory document,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpdateAsync(Guid id, ExpenseCategory document,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveAsync(Guid id,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<ExpenseCategory?> GetByNameAsync(string name, Guid userId) => Task.FromResult<ExpenseCategory?>(null);

    public Task<IReadOnlyList<ExpenseCategory>> SearchByNameAsync(string term, Guid userId) =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>(Array.Empty<ExpenseCategory>());
}
