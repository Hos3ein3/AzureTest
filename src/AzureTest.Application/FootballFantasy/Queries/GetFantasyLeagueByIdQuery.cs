using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetFantasyLeagueByIdQuery(Guid LeagueId) : IQuery<Result<FantasyLeagueDto>>;

public sealed class GetFantasyLeagueByIdQueryHandler : BaseQueryHandler<GetFantasyLeagueByIdQuery, Result<FantasyLeagueDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GetFantasyLeagueByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ICurrentUser currentUser, ILogger<GetFantasyLeagueByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    protected override async Task<Result<FantasyLeagueDto>> ExecuteAsync(GetFantasyLeagueByIdQuery request, CancellationToken cancellationToken)
    {
        var x = await _uow.FantasyLeagues.GetWithDetailsAsync(request.LeagueId, cancellationToken);
        if (x == null) return Result<FantasyLeagueDto>.Failure("League not found.");

        var currentUserId = _currentUser.Id;
        var poolRule = x.PoolRules.FirstOrDefault();
        var poolScopeSummary = poolRule != null
            ? (poolRule.ScopeType == AzureTest.Domain.Entities.FootballFantasy.Enums.PoolScopeType.All ? "Worldwide (All Players)" : $"{poolRule.ScopeType}: {poolRule.ScopeValue}")
            : "Worldwide (All Players)";

        var scoringRuleDtos = x.ScoringRuleSet?.RulesList?
            .Select(r => new ScoringRuleItemDto(r.Metric, r.Position, r.Points, r.Threshold))
            .ToList();

        var dto = new FantasyLeagueDto(
            x.Id,
            x.OwnerUserId,
            x.Name,
            x.LeagueTag,
            x.Description,
            x.Visibility,
            x.Mode,
            x.PeriodType,
            x.SquadMode,
            x.SquadSize,
            x.StartingSize,
            x.BenchSize,
            x.Budget,
            x.MaxPlayersPerRealTeam,
            x.AllowedFormations,
            x.CaptainEnabled,
            x.CaptainMultiplier,
            x.MaxMembers,
            x.Members.Count,
            x.JoinCode,
            x.ScoringRuleSetId,
            x.StartsAt,
            x.EndsAt,
            x.CloseTime,
            x.Status,
            currentUserId != Guid.Empty && x.FantasyTeams.Any(t => t.UserId == currentUserId),
            poolScopeSummary,
            poolRule?.ScopeType,
            poolRule?.ScopeValue,
            scoringRuleDtos,
            x.LogoUrl,
            x.AllowCoach,
            x.Window1Open,
            x.Window1Close,
            x.Window2Open,
            x.Window2Close,
            x.MinSquadSize,
            x.KickoutType,
            x.AuctionStartsAt,
            x.AuctionEndsAt,
            x.Format,
            x.AllowAutoSub,
            x.EnableMvpBonus,
            x.MvpBonusPoints,
            x.GroupCount,
            x.FreeTransfersPerGameweek,
            x.ExtraTransferPointPenalty,
            x.ProfitTaxPercent,
            x.GetNextTransferDeadline(),
            x.IsTransferMarketOpen()
        );

        return Result<FantasyLeagueDto>.Success(dto);
    }
}
