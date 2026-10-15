using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class HeadToHeadRepository : GenericRepository<HeadToHead>, IHeadToHeadRepository
{
    public HeadToHeadRepository(DbContext dbContext) : base(dbContext) { }
}
