using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeaguePositionLimitRepository : GenericRepository<FantasyLeaguePositionLimit>, IFantasyLeaguePositionLimitRepository
{
    public FantasyLeaguePositionLimitRepository(DbContext dbContext) : base(dbContext) { }
}
