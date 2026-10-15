using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.FootballFantasy;

namespace AzureTest.Application.Interfaces.Data;

public interface IPlayerRepository : IGenericRepository<Player>
{
    Task<IReadOnlyList<Player>> GetPlayersWithDetailsAsync(CancellationToken cancellationToken = default);
}
