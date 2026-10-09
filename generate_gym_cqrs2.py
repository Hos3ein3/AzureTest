import os

def write_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content.strip() + "\n")

write_file("src/AzureTest.Application/Gym/UserCourseCommandUnit.cs", """
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public record RegisterUserCourseCommand(Guid UserId, Guid CourseId) : ICommand<Result<Guid>>;
public class RegisterUserCourseCommandHandler : BaseCommandHandler<RegisterUserCourseCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public RegisterUserCourseCommandHandler(INoSqlUnitOfWork uow, ILogger<RegisterUserCourseCommandHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<Guid>> ExecuteAsync(RegisterUserCourseCommand request, CancellationToken ct) {
        var course = await _uow.TrainingCourses.GetByIdAsync(request.CourseId);
        if (course == null) return Result.Failure<Guid>("Course not found");
        
        var userCourse = new UserCourse {
            UserId = request.UserId,
            OriginalCourseId = course.Id,
            Name = course.Name,
            Weeks = course.Weeks // In a real app we'd deep copy if they are reference objects, but NoSQL driver will serialize them fine
        };
        await _uow.UserCourses.AddAsync(userCourse);
        return Result.Success(userCourse.Id);
    }
}
""")

write_file("src/AzureTest.Application/Gym/UserCourseQueryUnit.cs", """
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

public record GetUserCoursesQuery(Guid UserId) : IQuery<Result<List<UserCourse>>>;
public class GetUserCoursesQueryHandler : BaseQueryHandler<GetUserCoursesQuery, Result<List<UserCourse>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetUserCoursesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetUserCoursesQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<List<UserCourse>>> ExecuteAsync(GetUserCoursesQuery request, CancellationToken ct) {
        var items = await _uow.UserCourses.GetAllAsync();
        return Result.Success(items.Where(x => x.UserId == request.UserId).ToList());
    }
}

public record GetUserCourseByIdQuery(Guid Id) : IQuery<Result<UserCourse>>;
public class GetUserCourseByIdQueryHandler : BaseQueryHandler<GetUserCourseByIdQuery, Result<UserCourse>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetUserCourseByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetUserCourseByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<UserCourse>> ExecuteAsync(GetUserCourseByIdQuery request, CancellationToken ct) {
        var item = await _uow.UserCourses.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<UserCourse>("Not found");
    }
}
""")
