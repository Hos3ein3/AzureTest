using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureStatisticRepository : GenericRepository<FixtureStatistic>, IFixtureStatisticRepository
{
    public FixtureStatisticRepository(DbContext dbContext) : base(dbContext) { }
}
