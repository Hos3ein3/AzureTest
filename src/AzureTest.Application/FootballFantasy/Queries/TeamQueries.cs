using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetTeamsQuery(TeamFilter Filter) : IQuery<PagedResult<TeamDto, TeamFilter>>;

public sealed class GetTeamsQueryHandler : BaseQueryHandler<GetTeamsQuery, PagedResult<TeamDto, TeamFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetTeamsQueryHandler(IUnitOfWork uow, ILogger<GetTeamsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<TeamDto, TeamFilter>> ExecuteAsync(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new TeamFilter();
        var all = await _uow.Teams.GetAllAsync();
        var countries = await _uow.Countries.GetAllAsync();
        var countryMap = countries.ToDictionary(c => c.Id, c => c.Name);

        IEnumerable<Team> query = all;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s) || t.ShortName.ToLower().Contains(s) || (t.Code != null && t.Code.ToLower().Contains(s)));
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(t => t.CountryId == filter.CountryId.Value);
        }

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderBy(t => t.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.ShortName,
                t.Code,
                t.TeamType,
                t.CountryId,
                countryMap.TryGetValue(t.CountryId, out var cName) ? cName : "Unknown",
                t.FoundedYear,
                t.StadiumName,
                t.StadiumCapacity,
                t.LogoUrl
            ))
            .ToList();

        return new PagedResult<TeamDto, TeamFilter>(items, total, page, size, filter);
    }
}

public sealed record GetTeamByIdQuery(Guid Id) : IQuery<Result<TeamDto>>;

public sealed class GetTeamByIdQueryHandler : BaseQueryHandler<GetTeamByIdQuery, Result<TeamDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTeamByIdQueryHandler(IUnitOfWork uow, ILogger<GetTeamByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<TeamDto>> ExecuteAsync(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _uow.Teams.GetByIdAsync(request.Id);
        if (team == null) return Result<TeamDto>.Failure("Team not found.");

        var country = await _uow.Countries.GetByIdAsync(team.CountryId);

        return Result<TeamDto>.Success(new TeamDto(
            team.Id,
            team.Name,
            team.ShortName,
            team.Code,
            team.TeamType,
            team.CountryId,
            country?.Name ?? "Unknown",
            team.FoundedYear,
            team.StadiumName,
            team.StadiumCapacity,
            team.LogoUrl
        ));
    }
}
