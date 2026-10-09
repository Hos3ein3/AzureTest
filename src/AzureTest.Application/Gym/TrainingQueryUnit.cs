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

public record GetAllTrainingsQuery() : IQuery<Result<List<Training>>>;
public class GetAllTrainingsQueryHandler : BaseQueryHandler<GetAllTrainingsQuery, Result<List<Training>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetAllTrainingsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllTrainingsQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<List<Training>>> ExecuteAsync(GetAllTrainingsQuery request, CancellationToken ct) {
        var items = await _uow.Trainings.GetAllAsync();
        return Result.Success(items.ToList());
    }
}

public record GetTrainingByIdQuery(Guid Id) : IQuery<Result<Training>>;
public class GetTrainingByIdQueryHandler : BaseQueryHandler<GetTrainingByIdQuery, Result<Training>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetTrainingByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetTrainingByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<Training>> ExecuteAsync(GetTrainingByIdQuery request, CancellationToken ct) {
        var item = await _uow.Trainings.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<Training>("Not found");
    }
}
