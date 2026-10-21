using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoAccountRepository : GenericRepository<CryptoAccount>, ICryptoAccountRepository
{
    public CryptoAccountRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
