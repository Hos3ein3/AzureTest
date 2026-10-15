using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FixtureRepository : GenericRepository<Fixture>, IFixtureRepository
{
    public FixtureRepository(DbContext context) : base(context) { }
}
