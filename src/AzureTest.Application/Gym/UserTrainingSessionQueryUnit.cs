using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public record GetUserTrainingSessionsQuery(Guid UserId) : IQuery<Result<List<UserTrainingSession>>>;

public class GetUserTrainingSessionsQueryHandler : BaseQueryHandler<GetUserTrainingSessionsQuery, Result<List<UserTrainingSession>>> {
    private readonly INoSqlUnitOfWork _uow;

    public GetUserTrainingSessionsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetUserTrainingSessionsQueryHandler> logger) : base(logger) {
        _uow = uow;
    }

    protected override async Task<Result<List<UserTrainingSession>>> ExecuteAsync(GetUserTrainingSessionsQuery request, CancellationToken ct) {
        var all = await _uow.UserTrainingSessions.GetAllAsync();
        return Result.Success(all.Where(s => s.UserId == request.UserId).ToList());
    }
}
