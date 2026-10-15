using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyPeriodFixtureRepository : GenericRepository<FantasyPeriodFixture>, IFantasyPeriodFixtureRepository
{
    public FantasyPeriodFixtureRepository(DbContext dbContext) : base(dbContext) { }
}
