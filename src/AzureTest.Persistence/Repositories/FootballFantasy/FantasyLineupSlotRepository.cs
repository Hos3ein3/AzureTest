using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLineupSlotRepository : GenericRepository<FantasyLineupSlot>, IFantasyLineupSlotRepository
{
    public FantasyLineupSlotRepository(DbContext dbContext) : base(dbContext) { }
}
