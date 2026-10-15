using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class LeagueRepository : GenericRepository<League>, ILeagueRepository
{
    public LeagueRepository(DbContext context) : base(context) { }
}
