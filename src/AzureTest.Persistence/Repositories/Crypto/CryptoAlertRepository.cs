using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoAlertRepository : GenericRepository<CryptoAlert>, ICryptoAlertRepository
{
    public CryptoAlertRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
