

using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IDebtPersonRepository : IDocumentRepository<DebtPerson>
{
    Task<IReadOnlyList<DebtPerson>> SearchByNameAsync(string term);
}
