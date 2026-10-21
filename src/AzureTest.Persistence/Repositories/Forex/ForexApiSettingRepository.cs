using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexApiSettingRepository : GenericRepository<ForexApiSetting>, IForexApiSettingRepository
{
    public ForexApiSettingRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
