using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyPlayerScoreRepository : GenericRepository<FantasyPlayerScore>, IFantasyPlayerScoreRepository
{
    public FantasyPlayerScoreRepository(DbContext dbContext) : base(dbContext) { }
}
