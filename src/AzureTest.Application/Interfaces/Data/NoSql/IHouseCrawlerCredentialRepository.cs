using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IHouseCrawlerCredentialRepository : IDocumentRepository<HouseCrawlerCredential>
{
}
