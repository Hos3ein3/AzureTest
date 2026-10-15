using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class SeasonRepository : GenericRepository<Season>, ISeasonRepository
{
    public SeasonRepository(DbContext dbContext) : base(dbContext) { }
}
