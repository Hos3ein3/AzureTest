using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoJournalEntryRepository : GenericRepository<CryptoJournalEntry>, ICryptoJournalEntryRepository
{
    public CryptoJournalEntryRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
