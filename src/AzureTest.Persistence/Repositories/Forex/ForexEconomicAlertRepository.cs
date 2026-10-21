using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories.Forex;

public class ForexEconomicAlertRepository : GenericRepository<ForexEconomicAlert>, IForexEconomicAlertRepository
{
    public ForexEconomicAlertRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
