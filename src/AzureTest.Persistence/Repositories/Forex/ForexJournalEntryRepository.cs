using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexJournalEntryRepository : GenericRepository<ForexJournalEntry>, IForexJournalEntryRepository
{
    public ForexJournalEntryRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
