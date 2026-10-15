using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyPeriodRepository : GenericRepository<FantasyPeriod>, IFantasyPeriodRepository
{
    public FantasyPeriodRepository(DbContext dbContext) : base(dbContext) { }
}
