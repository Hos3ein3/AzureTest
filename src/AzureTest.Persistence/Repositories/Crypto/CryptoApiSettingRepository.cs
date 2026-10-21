using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Crypto;

public class CryptoApiSettingRepository : GenericRepository<CryptoApiSetting>, ICryptoApiSettingRepository
{
    public CryptoApiSettingRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
