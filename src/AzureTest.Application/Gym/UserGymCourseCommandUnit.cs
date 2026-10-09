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

public record RegisterUserGymCourseCommand(Guid UserId, Guid CourseId) : ICommand<Result<Guid>>;
public class RegisterUserGymCourseCommandHandler : ICommandHandler<RegisterUserGymCourseCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public RegisterUserGymCourseCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result<Guid>> HandleAsync(RegisterUserGymCourseCommand request, CancellationToken ct = default) {
        var course = await _uow.TrainingCourses.GetByIdAsync(request.CourseId);
        if (course == null) return Result.Failure<Guid>("Course not found");
        
        var userGymCourse = new UserGymCourse {
            UserId = request.UserId,
            OriginalCourseId = course.Id,
            Name = course.Name,
            Weeks = course.Weeks // In a real app we'd deep copy if they are reference objects, but NoSQL driver will serialize them fine
        };
        await _uow.UserGymCourses.AddAsync(userGymCourse);
        return Result.Success(userGymCourse.Id);
    }
}

public record UpdateUserGymCourseDayCommand(Guid UserId, Guid UserGymCourseId, int WeekNumber, CourseDay UpdatedDay) : ICommand<Result>;
public class UpdateUserGymCourseDayCommandHandler : ICommandHandler<UpdateUserGymCourseDayCommand, Result> {
    private readonly INoSqlUnitOfWork _uow;
    public UpdateUserGymCourseDayCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result> HandleAsync(UpdateUserGymCourseDayCommand request, CancellationToken ct = default) {
        var course = await _uow.UserGymCourses.GetByIdAsync(request.UserGymCourseId);
        if (course == null || course.UserId != request.UserId) return Result.Failure("Course not found or unauthorized");
        
        var week = course.Weeks.Find(w => w.WeekNumber == request.WeekNumber);
        if (week == null) return Result.Failure("Week not found");
        
        var dayIndex = week.Days.FindIndex(d => d.Id == request.UpdatedDay.Id);
        if (dayIndex == -1) return Result.Failure("Day not found");
        
        week.Days[dayIndex] = request.UpdatedDay;
        
        await _uow.UserGymCourses.UpdateAsync(course.Id, course);
        return Result.Success();
    }
}
