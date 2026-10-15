using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class ScoringRuleSetRepository : GenericRepository<ScoringRuleSet>, IScoringRuleSetRepository
{
    public ScoringRuleSetRepository(DbContext dbContext) : base(dbContext) { }
}
