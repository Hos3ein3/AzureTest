using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public record SaveUserTrainingSessionCommand(
    Guid UserId, 
    Guid UserGymCourseId, 
    Guid CourseDayId, 
    DateTime StartTime, 
    DateTime? EndTime, 
    int TotalDurationSeconds, 
    double TotalCaloriesBurned, 
    List<TrainingLog> Logs) : ICommand<Result<Guid>>;

public class SaveUserTrainingSessionCommandHandler : ICommandHandler<SaveUserTrainingSessionCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;

    public SaveUserTrainingSessionCommandHandler(INoSqlUnitOfWork uow) {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(SaveUserTrainingSessionCommand request, CancellationToken ct = default) {
        var session = new UserTrainingSession {
            UserId = request.UserId,
            UserGymCourseId = request.UserGymCourseId,
            CourseDayId = request.CourseDayId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TotalDurationSeconds = request.TotalDurationSeconds,
            TotalCaloriesBurned = request.TotalCaloriesBurned,
            Logs = request.Logs
        };
        await _uow.UserTrainingSessions.AddAsync(session);

        var userGymCourse = await _uow.UserGymCourses.GetByIdAsync(request.UserGymCourseId);
        if (userGymCourse != null && !userGymCourse.CompletedDayIds.Contains(request.CourseDayId)) {
            userGymCourse.CompletedDayIds.Add(request.CourseDayId);
            await _uow.UserGymCourses.UpdateAsync(userGymCourse.Id, userGymCourse);
        }

        return Result.Success(session.Id);
    }
}
