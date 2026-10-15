using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class SidelinedRepository : GenericRepository<Sidelined>, ISidelinedRepository
{
    public SidelinedRepository(DbContext dbContext) : base(dbContext) { }
}
