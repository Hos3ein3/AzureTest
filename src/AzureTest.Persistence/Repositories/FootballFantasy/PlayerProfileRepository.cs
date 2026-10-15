using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerProfileRepository : GenericRepository<PlayerProfile>, IPlayerProfileRepository
{
    public PlayerProfileRepository(DbContext dbContext) : base(dbContext) { }
}
