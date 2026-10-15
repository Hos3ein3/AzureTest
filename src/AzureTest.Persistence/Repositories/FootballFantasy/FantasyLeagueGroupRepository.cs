using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeagueGroupRepository : GenericRepository<FantasyLeagueGroup>, IFantasyLeagueGroupRepository
{
    public FantasyLeagueGroupRepository(DbContext dbContext) : base(dbContext) { }
}
