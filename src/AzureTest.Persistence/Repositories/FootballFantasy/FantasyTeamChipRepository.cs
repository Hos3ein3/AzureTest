using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyTeamChipRepository : GenericRepository<FantasyTeamChip>, IFantasyTeamChipRepository
{
    public FantasyTeamChipRepository(DbContext dbContext) : base(dbContext) { }
}
