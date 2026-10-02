using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IdeaPageRepository : DocumentRepository<IdeaPage>, IIdeaPageRepository
{
    public IdeaPageRepository(IMongoDatabase database) 
        : base(database, "IdeaPages")
    {
    }
}
