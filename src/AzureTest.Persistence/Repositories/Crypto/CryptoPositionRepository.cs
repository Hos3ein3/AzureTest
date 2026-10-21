using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoPositionRepository : GenericRepository<CryptoPosition>, ICryptoPositionRepository
{
    public CryptoPositionRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
