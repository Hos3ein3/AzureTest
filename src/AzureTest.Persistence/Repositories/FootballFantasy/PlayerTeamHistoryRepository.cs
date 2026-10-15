using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerTeamHistoryRepository : GenericRepository<PlayerTeamHistory>, IPlayerTeamHistoryRepository
{
    public PlayerTeamHistoryRepository(DbContext dbContext) : base(dbContext) { }
}
