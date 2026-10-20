using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullDebtPersonRepository : NullDocumentRepository<DebtPerson>, IDebtPersonRepository
{
    public Task<IReadOnlyList<DebtPerson>> SearchByNameAsync(string term) =>
        Task.FromResult<IReadOnlyList<DebtPerson>>(Array.Empty<DebtPerson>());
}
