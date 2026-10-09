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

public record GetAllTrainingCoursesQuery() : IQuery<Result<List<TrainingCourse>>>;
public class GetAllTrainingCoursesQueryHandler : BaseQueryHandler<GetAllTrainingCoursesQuery, Result<List<TrainingCourse>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetAllTrainingCoursesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllTrainingCoursesQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<List<TrainingCourse>>> ExecuteAsync(GetAllTrainingCoursesQuery request, CancellationToken ct) {
        var items = await _uow.TrainingCourses.GetAllAsync();
        return Result.Success(items.ToList());
    }
}

public record GetTrainingCourseByIdQuery(Guid Id) : IQuery<Result<TrainingCourse>>;
public class GetTrainingCourseByIdQueryHandler : BaseQueryHandler<GetTrainingCourseByIdQuery, Result<TrainingCourse>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetTrainingCourseByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetTrainingCourseByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<TrainingCourse>> ExecuteAsync(GetTrainingCourseByIdQuery request, CancellationToken ct) {
        var item = await _uow.TrainingCourses.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<TrainingCourse>("Not found");
    }
}
