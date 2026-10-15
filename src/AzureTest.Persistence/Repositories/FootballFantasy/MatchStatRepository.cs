using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class MatchStatRepository : GenericRepository<MatchStat>, IMatchStatRepository
{
    public MatchStatRepository(DbContext dbContext) : base(dbContext) { }
}
