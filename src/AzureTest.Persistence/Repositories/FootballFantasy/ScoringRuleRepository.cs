using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class ScoringRuleRepository : GenericRepository<ScoringRule>, IScoringRuleRepository
{
    public ScoringRuleRepository(DbContext context) : base(context) { }
}
