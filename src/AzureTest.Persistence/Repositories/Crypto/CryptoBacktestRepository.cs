using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoBacktestRepository : GenericRepository<CryptoBacktest>, ICryptoBacktestRepository
{
    public CryptoBacktestRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
