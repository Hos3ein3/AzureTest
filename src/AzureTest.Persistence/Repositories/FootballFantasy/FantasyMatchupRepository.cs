using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyMatchupRepository : GenericRepository<FantasyMatchup>, IFantasyMatchupRepository
{
    public FantasyMatchupRepository(DbContext dbContext) : base(dbContext) { }
}
