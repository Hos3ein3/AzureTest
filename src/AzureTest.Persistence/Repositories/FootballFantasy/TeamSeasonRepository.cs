using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TeamSeasonRepository : GenericRepository<TeamSeason>, ITeamSeasonRepository
{
    public TeamSeasonRepository(DbContext dbContext) : base(dbContext) { }
}
