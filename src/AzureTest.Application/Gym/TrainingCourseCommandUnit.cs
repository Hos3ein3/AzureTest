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

public record CreateTrainingCourseCommand(string Name, string Description, List<string> Tags, List<CourseWeek> Weeks, string? ImagePath = null, string? VideoPath = null) : ICommand<Result<Guid>>;
public class CreateTrainingCourseCommandValidator : AbstractValidator<CreateTrainingCourseCommand> {
    public CreateTrainingCourseCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class CreateTrainingCourseCommandHandler : ICommandHandler<CreateTrainingCourseCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public CreateTrainingCourseCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result<Guid>> HandleAsync(CreateTrainingCourseCommand request, CancellationToken ct = default) {
        var course = new TrainingCourse { Name = request.Name, Description = request.Description, Tags = request.Tags ?? new(), Weeks = request.Weeks, ImagePath = request.ImagePath, VideoPath = request.VideoPath };
        await _uow.TrainingCourses.AddAsync(course);
        return Result.Success(course.Id);
    }
}

public record UpdateTrainingCourseCommand(Guid Id, string Name, string Description, List<string> Tags, List<CourseWeek> Weeks, string? ImagePath = null, string? VideoPath = null) : ICommand<Result>;
public class UpdateTrainingCourseCommandValidator : AbstractValidator<UpdateTrainingCourseCommand> {
    public UpdateTrainingCourseCommandValidator() {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class UpdateTrainingCourseCommandHandler : ICommandHandler<UpdateTrainingCourseCommand, Result> {
    private readonly INoSqlUnitOfWork _uow;
    public UpdateTrainingCourseCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result> HandleAsync(UpdateTrainingCourseCommand request, CancellationToken ct = default) {
        var course = await _uow.TrainingCourses.GetByIdAsync(request.Id);
        if (course == null) return Result.Failure("Not found");
        course.Name = request.Name;
        course.Description = request.Description;
        course.Tags = request.Tags ?? new();
        course.Weeks = request.Weeks ?? new();
        course.ImagePath = request.ImagePath;
        course.VideoPath = request.VideoPath;
        await _uow.TrainingCourses.UpdateAsync(request.Id, course);
        return Result.Success();
    }
}
