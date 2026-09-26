using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Queries.GetResumeStatusLogsByResumeId;

public sealed class GetResumeStatusLogsByResumeIdQueryHandler : BaseQueryHandler<GetResumeStatusLogsByResumeIdQuery, IReadOnlyList<ResumeStatusLogDto>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetResumeStatusLogsByResumeIdQueryHandler(
        INoSqlUnitOfWork noSqlUnitOfWork,
        ILogger<GetResumeStatusLogsByResumeIdQueryHandler> logger)
        : base(logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    protected override async Task<IReadOnlyList<ResumeStatusLogDto>> ExecuteAsync(
        GetResumeStatusLogsByResumeIdQuery query,
        CancellationToken cancellationToken)
    {
        var logs = await _noSqlUnitOfWork.ResumeStatusLogs.GetByResumeIdAsync(query.ResumeId);

        return logs.Select(log => new ResumeStatusLogDto(
            log.Id,
            log.ResumeId,
            log.Date,
            log.OldStatus,
            log.NewStatus,
            log.Note
        )).ToList();
    }
}
