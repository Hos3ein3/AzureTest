using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureLiveUpdateRepository : GenericRepository<FixtureLiveUpdate>, IFixtureLiveUpdateRepository
{
    public FixtureLiveUpdateRepository(DbContext dbContext) : base(dbContext) { }
}
