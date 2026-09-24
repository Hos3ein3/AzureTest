using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Queries.GetStatusLogsByResumeId;

public sealed class GetStatusLogsByResumeIdQueryHandler : BaseQueryHandler<GetStatusLogsByResumeIdQuery, IReadOnlyList<StatusLogDto>>
{
    private readonly IStatusLogRepository _statusLogRepository;

    public GetStatusLogsByResumeIdQueryHandler(
        IStatusLogRepository statusLogRepository,
        ILogger<GetStatusLogsByResumeIdQueryHandler> logger)
        : base(logger)
    {
        _statusLogRepository = statusLogRepository;
    }

    protected override async Task<IReadOnlyList<StatusLogDto>> ExecuteAsync(
        GetStatusLogsByResumeIdQuery query,
        CancellationToken cancellationToken)
    {
        var logs = await _statusLogRepository.GetByResumeIdAsync(query.ResumeId);

        return logs.Select(log => new StatusLogDto(
            log.Id,
            log.ResumeId,
            log.Date,
            log.OldStatus,
            log.NewStatus,
            log.Note
        )).ToList();
    }
}
