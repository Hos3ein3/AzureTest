using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerMatchStatRepository : GenericRepository<PlayerMatchStat>, IPlayerMatchStatRepository
{
    public PlayerMatchStatRepository(DbContext dbContext) : base(dbContext) { }
}
