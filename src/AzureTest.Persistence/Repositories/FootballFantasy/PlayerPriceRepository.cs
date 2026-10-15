using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerPriceRepository : GenericRepository<PlayerPrice>, IPlayerPriceRepository
{
    public PlayerPriceRepository(DbContext dbContext) : base(dbContext) { }
}
