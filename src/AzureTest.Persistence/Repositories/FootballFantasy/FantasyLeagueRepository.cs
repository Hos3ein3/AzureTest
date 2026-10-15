using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyLeagueRepository : GenericRepository<FantasyLeague>, IFantasyLeagueRepository
{
    public FantasyLeagueRepository(DbContext context) : base(context) { }

    public async Task<FantasyLeague?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.ScoringRuleSet)
                .ThenInclude(s => s.RulesList)
            .Include(x => x.PositionLimits)
            .Include(x => x.Sources)
            .Include(x => x.PoolRules)
            .Include(x => x.Members)
            .Include(x => x.Invites)
            .Include(x => x.Periods)
            .Include(x => x.Groups)
                .ThenInclude(g => g.Matchups)
            .Include(x => x.FantasyTeams)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FantasyLeague>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.ScoringRuleSet)
                .ThenInclude(s => s.RulesList)
            .Include(x => x.PositionLimits)
            .Include(x => x.Sources)
            .Include(x => x.PoolRules)
            .Include(x => x.Members)
            .Include(x => x.FantasyTeams)
            .ToListAsync(cancellationToken);
    }
}
