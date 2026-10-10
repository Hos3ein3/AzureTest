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
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPredictClubsQuery(PredictClubFilter Filter) : IQuery<PagedResult<PredictClubDto, PredictClubFilter>>;

public sealed class GetPredictClubsQueryValidator : AbstractValidator<GetPredictClubsQuery>
{
    public GetPredictClubsQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull();
    }
}

public sealed class GetPredictClubsQueryHandler : IQueryHandler<GetPredictClubsQuery, PagedResult<PredictClubDto, PredictClubFilter>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GetPredictClubsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PredictClubDto, PredictClubFilter>> HandleAsync(GetPredictClubsQuery query, CancellationToken cancellationToken = default)
    {
        var filter = query.Filter ?? new PredictClubFilter();
        var allClubs = await _uow.PredictClubs.GetAllAsync();
        var allMembers = await _uow.PredictClubMembers.GetAllAsync();

        var userClubIds = allMembers
            .Where(m => m.UserId == _currentUser.Id && m.Status == FantasyMemberStatus.Active)
            .Select(m => m.PredictClubId)
            .ToHashSet();

        // Security / Visibility rule:
        // Global and Public clubs are visible.
        // Private clubs are visible ONLY if user is a member OR owner.
        var queryable = allClubs.Where(c =>
            c.Visibility == FantasyLeagueVisibility.Global ||
            c.Visibility == FantasyLeagueVisibility.Public ||
            userClubIds.Contains(c.Id) ||
            c.OwnerUserId == _currentUser.Id
        );

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLowerInvariant();
            queryable = queryable.Where(c =>
                c.Name.ToLowerInvariant().Contains(term) ||
                c.ClubTag.ToLowerInvariant().Contains(term)
            );
        }

        if (filter.Visibility.HasValue)
        {
            queryable = queryable.Where(c => c.Visibility == filter.Visibility.Value);
        }

        if (filter.Status.HasValue)
        {
            queryable = queryable.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.ScopeType.HasValue)
        {
            queryable = queryable.Where(c => c.ScopeType == filter.ScopeType.Value);
        }

        var list = queryable.ToList();
        var totalCount = list.Count;
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 12 : filter.PageSize;

        var items = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c =>
            {
                var count = allMembers.Count(m => m.PredictClubId == c.Id && m.Status == FantasyMemberStatus.Active);
                return new PredictClubDto(
                    c.Id,
                    c.OwnerUserId,
                    c.Name,
                    c.ClubTag,
                    c.Description,
                    c.LogoUrl,
                    c.Visibility,
                    c.Format,
                    c.ScopeType,
                    c.ScopeValue,
                    c.MaxMembers,
                    count,
                    c.JoinCode,
                    c.Status,
                    c.StartsAt,
                    c.EndsAt,
                    c.ExactScorePoints,
                    c.OutcomePoints,
                    c.HalfTimeScorePoints,
                    c.BothTeamsToScorePoints,
                    c.OverUnderPoints,
                    c.YellowCardsPoints,
                    c.CornersPoints,
                    c.MatchMvpPoints,
                    c.FirstGoalMinutePoints,
                    c.EnableTournamentPredictions,
                    c.TournamentPodiumPoints,
                    c.TournamentAwardPoints,
                    c.PredictExactScore,
                    c.PredictOutcome,
                    c.PredictHalfTime,
                    c.PredictBtts,
                    c.PredictCards,
                    c.PredictCorners,
                    c.PredictMvp
                );
            })
            .ToList();

        return new PagedResult<PredictClubDto, PredictClubFilter>(items, totalCount, pageNumber, pageSize, filter);
    }
}
