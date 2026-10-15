using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLineupRepository : GenericRepository<FantasyLineup>, IFantasyLineupRepository
{
    public FantasyLineupRepository(DbContext dbContext) : base(dbContext) { }
}
