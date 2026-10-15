using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class BetTypeRepository : GenericRepository<BetType>, IBetTypeRepository
{
    public BetTypeRepository(DbContext dbContext) : base(dbContext) { }
}
