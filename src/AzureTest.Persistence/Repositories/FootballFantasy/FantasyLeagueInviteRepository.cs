using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeagueInviteRepository : GenericRepository<FantasyLeagueInvite>, IFantasyLeagueInviteRepository
{
    public FantasyLeagueInviteRepository(DbContext dbContext) : base(dbContext) { }
}
