using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeagueSourceRepository : GenericRepository<FantasyLeagueSource>, IFantasyLeagueSourceRepository
{
    public FantasyLeagueSourceRepository(DbContext dbContext) : base(dbContext) { }
}
