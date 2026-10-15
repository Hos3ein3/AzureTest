using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerInjuryRepository : GenericRepository<PlayerInjury>, IPlayerInjuryRepository
{
    public PlayerInjuryRepository(DbContext dbContext) : base(dbContext) { }
}
