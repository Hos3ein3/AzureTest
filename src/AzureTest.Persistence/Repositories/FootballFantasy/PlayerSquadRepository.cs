using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerSquadRepository : GenericRepository<PlayerSquad>, IPlayerSquadRepository
{
    public PlayerSquadRepository(DbContext dbContext) : base(dbContext) { }
}
