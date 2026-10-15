using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyTeamPeriodScoreRepository : GenericRepository<FantasyTeamPeriodScore>, IFantasyTeamPeriodScoreRepository
{
    public FantasyTeamPeriodScoreRepository(DbContext dbContext) : base(dbContext) { }
}
