using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeaguePoolRuleRepository : GenericRepository<FantasyLeaguePoolRule>, IFantasyLeaguePoolRuleRepository
{
    public FantasyLeaguePoolRuleRepository(DbContext dbContext) : base(dbContext) { }
}
