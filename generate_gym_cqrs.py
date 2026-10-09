import os

def write_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content.strip() + "\n")

write_file("src/AzureTest.Application/Gym/TrainingCommandUnit.cs", """
using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using AzureTest.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public record CreateTrainingCommand(string Name, string Description, string MediaUrl, TrainingType Type, double BaseCaloriesBurn) : ICommand<Result<Guid>>;
public class CreateTrainingCommandValidator : AbstractValidator<CreateTrainingCommand> {
    public CreateTrainingCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class CreateTrainingCommandHandler : BaseCommandHandler<CreateTrainingCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public CreateTrainingCommandHandler(INoSqlUnitOfWork uow, ILogger<CreateTrainingCommandHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<Guid>> ExecuteAsync(CreateTrainingCommand request, CancellationToken ct) {
        var training = new Training { Name = request.Name, Description = request.Description, MediaUrl = request.MediaUrl, Type = request.Type, BaseCaloriesBurn = request.BaseCaloriesBurn };
        await _uow.Trainings.AddAsync(training);
        return Result.Success(training.Id);
    }
}

public record UpdateTrainingCommand(Guid Id, string Name, string Description, string MediaUrl, TrainingType Type, double BaseCaloriesBurn) : ICommand<Result>;
public class UpdateTrainingCommandValidator : AbstractValidator<UpdateTrainingCommand> {
    public UpdateTrainingCommandValidator() {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class UpdateTrainingCommandHandler : BaseCommandHandler<UpdateTrainingCommand, Result> {
    private readonly INoSqlUnitOfWork _uow;
    public UpdateTrainingCommandHandler(INoSqlUnitOfWork uow, ILogger<UpdateTrainingCommandHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result> ExecuteAsync(UpdateTrainingCommand request, CancellationToken ct) {
        var training = await _uow.Trainings.GetByIdAsync(request.Id);
        if (training == null) return Result.Failure("Not found");
        training.Name = request.Name;
        training.Description = request.Description;
        training.MediaUrl = request.MediaUrl;
        training.Type = request.Type;
        training.BaseCaloriesBurn = request.BaseCaloriesBurn;
        await _uow.Trainings.UpdateAsync(request.Id, training);
        return Result.Success();
    }
}
""")

write_file("src/AzureTest.Application/Gym/TrainingQueryUnit.cs", """
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
""")

write_file("src/AzureTest.Application/Gym/TrainingCourseCommandUnit.cs", """
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

public record CreateTrainingCourseCommand(string Name, string Description, List<string> Tags, List<CourseWeek> Weeks) : ICommand<Result<Guid>>;
public class CreateTrainingCourseCommandValidator : AbstractValidator<CreateTrainingCourseCommand> {
    public CreateTrainingCourseCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class CreateTrainingCourseCommandHandler : BaseCommandHandler<CreateTrainingCourseCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public CreateTrainingCourseCommandHandler(INoSqlUnitOfWork uow, ILogger<CreateTrainingCourseCommandHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<Guid>> ExecuteAsync(CreateTrainingCourseCommand request, CancellationToken ct) {
        var course = new TrainingCourse { Name = request.Name, Description = request.Description, Tags = request.Tags ?? new(), Weeks = request.Weeks ?? new() };
        await _uow.TrainingCourses.AddAsync(course);
        return Result.Success(course.Id);
    }
}

public record UpdateTrainingCourseCommand(Guid Id, string Name, string Description, List<string> Tags, List<CourseWeek> Weeks) : ICommand<Result>;
public class UpdateTrainingCourseCommandValidator : AbstractValidator<UpdateTrainingCourseCommand> {
    public UpdateTrainingCourseCommandValidator() {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class UpdateTrainingCourseCommandHandler : BaseCommandHandler<UpdateTrainingCourseCommand, Result> {
    private readonly INoSqlUnitOfWork _uow;
    public UpdateTrainingCourseCommandHandler(INoSqlUnitOfWork uow, ILogger<UpdateTrainingCourseCommandHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result> ExecuteAsync(UpdateTrainingCourseCommand request, CancellationToken ct) {
        var course = await _uow.TrainingCourses.GetByIdAsync(request.Id);
        if (course == null) return Result.Failure("Not found");
        course.Name = request.Name;
        course.Description = request.Description;
        course.Tags = request.Tags ?? new();
        course.Weeks = request.Weeks ?? new();
        await _uow.TrainingCourses.UpdateAsync(request.Id, course);
        return Result.Success();
    }
}
""")

write_file("src/AzureTest.Application/Gym/TrainingCourseQueryUnit.cs", """
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
""")
