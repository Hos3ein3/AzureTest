

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface ICompanyRepository : IDocumentRepository<Company>
{
    Task<Company?> GetByNameAsync(string companyName);
    Task<IReadOnlyList<Company>> SearchByNameAsync(string term);
}
