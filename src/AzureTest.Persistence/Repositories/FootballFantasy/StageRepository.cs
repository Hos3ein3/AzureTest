using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class StageRepository : GenericRepository<Stage>, IStageRepository
{
    public StageRepository(DbContext dbContext) : base(dbContext) { }
}
