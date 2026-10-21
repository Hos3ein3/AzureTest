using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoStrategyRepository : GenericRepository<CryptoStrategy>, ICryptoStrategyRepository
{
    public CryptoStrategyRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
