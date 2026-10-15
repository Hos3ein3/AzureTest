using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerSeasonStatisticRepository : GenericRepository<PlayerSeasonStatistic>, IPlayerSeasonStatisticRepository
{
    public PlayerSeasonStatisticRepository(DbContext dbContext) : base(dbContext) { }
}
