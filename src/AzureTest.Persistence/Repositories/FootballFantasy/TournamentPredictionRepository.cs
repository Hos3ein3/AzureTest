using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TournamentPredictionRepository : GenericRepository<TournamentPrediction>, ITournamentPredictionRepository
{
    public TournamentPredictionRepository(DbContext dbContext) : base(dbContext) { }
}
