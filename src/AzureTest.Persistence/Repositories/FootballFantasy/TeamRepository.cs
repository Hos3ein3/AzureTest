using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(DbContext context) : base(context) { }
}
