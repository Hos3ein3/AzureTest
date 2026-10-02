using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IdeaSectionRepository : DocumentRepository<IdeaSection>, IIdeaSectionRepository
{
    public IdeaSectionRepository(IMongoDatabase database) 
        : base(database, "IdeaSections")
    {
    }
}
