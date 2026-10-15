using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
{
    public PlayerRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Player>> GetPlayersWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Country)
            .Include(x => x.PlayerSquads)
                .ThenInclude(ps => ps.TeamSeason)
                    .ThenInclude(ts => ts.Team)
            .Include(x => x.PlayerSquads)
                .ThenInclude(ps => ps.TeamSeason)
                    .ThenInclude(ts => ts.Season)
                        .ThenInclude(s => s.League)
            .Include(x => x.Prices)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
