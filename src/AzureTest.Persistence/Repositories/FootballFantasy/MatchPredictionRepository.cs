using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class MatchPredictionRepository : GenericRepository<MatchPrediction>, IMatchPredictionRepository
{
    public MatchPredictionRepository(DbContext dbContext) : base(dbContext) { }
}
