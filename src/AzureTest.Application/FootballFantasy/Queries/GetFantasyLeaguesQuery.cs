using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetFantasyLeaguesQuery(FantasyLeagueFilter Filter) : IQuery<PagedResult<FantasyLeagueDto, FantasyLeagueFilter>>;

public sealed class GetFantasyLeaguesQueryHandler : BaseQueryHandler<GetFantasyLeaguesQuery, PagedResult<FantasyLeagueDto, FantasyLeagueFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GetFantasyLeaguesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ICurrentUser currentUser, ILogger<GetFantasyLeaguesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    protected override async Task<PagedResult<FantasyLeagueDto, FantasyLeagueFilter>> ExecuteAsync(GetFantasyLeaguesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new FantasyLeagueFilter();
        var all = await _uow.FantasyLeagues.GetAllWithDetailsAsync(cancellationToken);
        var query = all.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.LeagueTag.ToLower().Contains(search));
        }

        var currentUserId = _currentUser.Id;
        var currentUserEmail = _currentUser.Email?.ToLower();
        var isAdmin = _currentUser.IsAdmin;

        // Rule: Private leagues do not appear to normal users unless they are the owner, a member, or have received an invite
        query = query.Where(x => 
            x.Visibility != AzureTest.Domain.Entities.FootballFantasy.Enums.FantasyLeagueVisibility.Private 
            || isAdmin
            || (currentUserId != Guid.Empty && (
                x.OwnerUserId == currentUserId 
                || x.Members.Any(m => m.UserId == currentUserId)
                || (!string.IsNullOrEmpty(currentUserEmail) && x.Invites.Any(i => i.Email != null && i.Email.ToLower() == currentUserEmail))
            ))
        );

        if (filter.Visibility.HasValue)
            query = query.Where(x => x.Visibility == filter.Visibility.Value);

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

        query = filter.SortBy switch
        {
            "Name" => filter.SortDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "LeagueTag" => filter.SortDescending ? query.OrderByDescending(x => x.LeagueTag) : query.OrderBy(x => x.LeagueTag),
            _ => filter.SortDescending ? query.OrderByDescending(x => x.StartsAt) : query.OrderBy(x => x.StartsAt)
        };

        var totalCount = query.Count();
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 12 : filter.PageSize;

        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FantasyLeagueDto(
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
                null,
                null,
                null,
                null,
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
            ))
            .ToList();

        return new PagedResult<FantasyLeagueDto, FantasyLeagueFilter>(items, totalCount, pageNumber, pageSize, filter);
    }
}
