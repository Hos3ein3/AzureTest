using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.FootballFantasy;

namespace AzureTest.Application.Interfaces.Data;

public interface IFantasyLeagueRepository : IGenericRepository<FantasyLeague>
{
    Task<FantasyLeague?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FantasyLeague>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}
