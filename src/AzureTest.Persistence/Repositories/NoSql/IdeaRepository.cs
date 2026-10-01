using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IdeaRepository : DocumentRepository<Idea>, IIdeaRepository
{
    public IdeaRepository(IMongoDatabase database) 
        : base(database, "Ideas")
    {
    }
}
