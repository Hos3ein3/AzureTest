using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerTopScorerRepository : GenericRepository<PlayerTopScorer>, IPlayerTopScorerRepository
{
    public PlayerTopScorerRepository(DbContext dbContext) : base(dbContext) { }
}
