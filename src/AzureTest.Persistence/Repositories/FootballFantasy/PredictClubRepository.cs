using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PredictClubRepository : GenericRepository<PredictClub>, IPredictClubRepository
{
    public PredictClubRepository(DbContext context) : base(context) { }

    public async Task<PredictClub?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Members)
            .Include(x => x.Predictions)
                .ThenInclude(p => p.Fixture)
                    .ThenInclude(f => f.HomeTeam)
            .Include(x => x.Predictions)
                .ThenInclude(p => p.Fixture)
                    .ThenInclude(f => f.AwayTeam)
            .Include(x => x.TournamentPredictions)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
