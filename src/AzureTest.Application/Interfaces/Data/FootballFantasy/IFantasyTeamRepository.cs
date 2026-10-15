using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.FootballFantasy;

namespace AzureTest.Application.Interfaces.Data;

public interface IFantasyTeamRepository : IGenericRepository<FantasyTeam>
{
    Task<FantasyTeam?> GetWithLineupsAndSquadAsync(Guid leagueId, Guid userId, CancellationToken cancellationToken = default);
    Task<FantasyTeam?> GetByIdWithDetailsAsync(Guid teamId, CancellationToken cancellationToken = default);
}
