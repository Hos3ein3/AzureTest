using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class FantasyTeamRepository : GenericRepository<FantasyTeam>, IFantasyTeamRepository
{
    public FantasyTeamRepository(DbContext context) : base(context) { }

    public async Task<FantasyTeam?> GetByIdWithDetailsAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.FantasyLeague)
            .Include(x => x.SquadEntries)
                .ThenInclude(se => se.Player)
                    .ThenInclude(p => p.PlayerSquads)
                        .ThenInclude(ps => ps.TeamSeason)
                            .ThenInclude(ts => ts.Team)
            .Include(x => x.Lineups)
                .ThenInclude(l => l.Slots)
                    .ThenInclude(sl => sl.Player)
            .Include(x => x.Chips)
            .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    }

    public async Task<FantasyTeam?> GetWithLineupsAndSquadAsync(Guid leagueId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.SquadEntries)
                .ThenInclude(se => se.Player)
                    .ThenInclude(p => p.PlayerSquads)
                        .ThenInclude(ps => ps.TeamSeason)
                            .ThenInclude(ts => ts.Team)
            .Include(x => x.Lineups)
                .ThenInclude(l => l.Slots)
                    .ThenInclude(sl => sl.Player)
            .Include(x => x.Chips)
            .FirstOrDefaultAsync(x => x.FantasyLeagueId == leagueId && x.UserId == userId, cancellationToken);
    }
}
