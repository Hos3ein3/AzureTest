using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Entities.Pomodoro;

namespace AzureTest.Application.Pomodoro.Queries.GetPomodoroSessions;

public sealed record GetPomodoroSessionsQuery(Guid UserId, PomodoroFilter Filter) : IQuery<Result<IReadOnlyList<PomodoroSessionDto>>>
{
    public static class Mapping
    {
        public static PomodoroSessionDto ToDto(PomodoroSession s) =>
            new(
                s.Id,
                s.UserId,
                s.Title,
                s.Category,
                s.SessionType,
                s.Status,
                s.PlannedDurationSeconds,
                s.ActualDurationSeconds,
                s.StartedAtUtc,
                s.EndedAtUtc,
                s.InterruptionCount,
                s.Notes,
                s.UsedSkin,
                s.CreatedOn);
    }
}

public sealed class GetPomodoroSessionsQueryHandler : IQueryHandler<GetPomodoroSessionsQuery, Result<IReadOnlyList<PomodoroSessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPomodoroSessionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PomodoroSessionDto>>> HandleAsync(GetPomodoroSessionsQuery query, CancellationToken cancellationToken = default)
    {
        var sessions = await _unitOfWork.Pomodoros.GetUserSessionsAsync(
            query.UserId,
            query.Filter.FromDate,
            query.Filter.ToDate,
            cancellationToken);

        var filtered = sessions.AsEnumerable();

        if (query.Filter.SessionType.HasValue)
        {
            filtered = filtered.Where(s => s.SessionType == query.Filter.SessionType.Value);
        }

        if (query.Filter.Status.HasValue)
        {
            filtered = filtered.Where(s => s.Status == query.Filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Filter.Category))
        {
            filtered = filtered.Where(s => string.Equals(s.Category, query.Filter.Category, StringComparison.OrdinalIgnoreCase));
        }

        var pageSize = Math.Max(1, query.Filter.PageSize);
        var page = Math.Max(1, query.Filter.Page);

        var dtos = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(GetPomodoroSessionsQuery.Mapping.ToDto)
            .ToList();

        return Result<IReadOnlyList<PomodoroSessionDto>>.Success(dtos);
    }
}
