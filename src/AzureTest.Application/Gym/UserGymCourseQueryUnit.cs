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

public record GetUserGymCoursesQuery(Guid UserId) : IQuery<Result<List<UserGymCourse>>>;
public class GetUserGymCoursesQueryHandler : BaseQueryHandler<GetUserGymCoursesQuery, Result<List<UserGymCourse>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetUserGymCoursesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetUserGymCoursesQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<List<UserGymCourse>>> ExecuteAsync(GetUserGymCoursesQuery request, CancellationToken ct) {
        var items = await _uow.UserGymCourses.GetAllAsync();
        return Result.Success(items.Where(x => x.UserId == request.UserId).ToList());
    }
}

public record GetUserGymCourseByIdQuery(Guid Id) : IQuery<Result<UserGymCourse>>;
public class GetUserGymCourseByIdQueryHandler : BaseQueryHandler<GetUserGymCourseByIdQuery, Result<UserGymCourse>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetUserGymCourseByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetUserGymCourseByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<UserGymCourse>> ExecuteAsync(GetUserGymCourseByIdQuery request, CancellationToken ct) {
        var item = await _uow.UserGymCourses.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<UserGymCourse>("Not found");
    }
}
