using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureEventRepository : GenericRepository<FixtureEvent>, IFixtureEventRepository
{
    public FixtureEventRepository(DbContext dbContext) : base(dbContext) { }
}
