using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TeamStatisticRepository : GenericRepository<TeamStatistic>, ITeamStatisticRepository
{
    public TeamStatisticRepository(DbContext dbContext) : base(dbContext) { }
}
