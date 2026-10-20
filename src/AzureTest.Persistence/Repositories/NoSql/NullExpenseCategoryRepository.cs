using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IExpenseCategoryRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullExpenseCategoryRepository : NullDocumentRepository<ExpenseCategory>, IExpenseCategoryRepository
{
    public Task<ExpenseCategory?> GetByNameAsync(string name, Guid userId) => Task.FromResult<ExpenseCategory?>(null);

    public Task<IReadOnlyList<ExpenseCategory>> SearchByNameAsync(string term, Guid userId) =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>(Array.Empty<ExpenseCategory>());
}
