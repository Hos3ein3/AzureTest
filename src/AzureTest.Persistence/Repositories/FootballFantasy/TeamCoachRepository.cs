using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TeamCoachRepository : GenericRepository<TeamCoach>, ITeamCoachRepository
{
    public TeamCoachRepository(DbContext dbContext) : base(dbContext) { }
}
