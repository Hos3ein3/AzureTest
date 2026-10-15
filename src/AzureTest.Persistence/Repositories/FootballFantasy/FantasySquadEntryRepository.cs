using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasySquadEntryRepository : GenericRepository<FantasySquadEntry>, IFantasySquadEntryRepository
{
    public FantasySquadEntryRepository(DbContext dbContext) : base(dbContext) { }
}
