using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoOrderRepository : GenericRepository<CryptoOrder>, ICryptoOrderRepository
{
    public CryptoOrderRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
