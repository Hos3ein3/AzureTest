using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureLineupRepository : GenericRepository<FixtureLineup>, IFixtureLineupRepository
{
    public FixtureLineupRepository(DbContext dbContext) : base(dbContext) { }
}
