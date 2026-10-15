using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class LeagueStandingRepository : GenericRepository<LeagueStanding>, ILeagueStandingRepository
{
    public LeagueStandingRepository(DbContext dbContext) : base(dbContext) { }
}
