using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IExpenseCategoryRepository : IDocumentRepository<ExpenseCategory>
{
    Task<ExpenseCategory?> GetByNameAsync(string name, Guid userId);
    Task<IReadOnlyList<ExpenseCategory>> SearchByNameAsync(string term, Guid userId);
}
