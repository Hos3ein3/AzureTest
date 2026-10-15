using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureOddRepository : GenericRepository<FixtureOdd>, IFixtureOddRepository
{
    public FixtureOddRepository(DbContext dbContext) : base(dbContext) { }
}
