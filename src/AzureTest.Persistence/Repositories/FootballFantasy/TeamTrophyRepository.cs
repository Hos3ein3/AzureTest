using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TeamTrophyRepository : GenericRepository<TeamTrophy>, ITeamTrophyRepository
{
    public TeamTrophyRepository(DbContext dbContext) : base(dbContext) { }
}
