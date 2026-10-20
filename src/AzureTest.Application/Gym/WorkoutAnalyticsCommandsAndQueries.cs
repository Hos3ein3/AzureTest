using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Gym;
using AzureTest.Domain.Documents.Gym;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

// ── Log Weight Command ─────────────────────────────────────────────────────────

public record LogUserWeightCommand(
    Guid UserId,
    double WeightKg,
    double HeightCm,
    DateTime Date,
    string? Notes,
    double? MusclePercent = null,
    double? BonePercent = null,
    double? FatPercent = null) : ICommand<Result<Guid>>;

public class LogUserWeightCommandValidator : AbstractValidator<LogUserWeightCommand>
{
    public LogUserWeightCommandValidator()
    {
        RuleFor(x => x.WeightKg)
            .InclusiveBetween(UserWeightLog.Rules.MinWeightKg, UserWeightLog.Rules.MaxWeightKg)
            .WithMessage($"Weight must be between {UserWeightLog.Rules.MinWeightKg}kg and {UserWeightLog.Rules.MaxWeightKg}kg.");

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(UserWeightLog.Rules.MinHeightCm, UserWeightLog.Rules.MaxHeightCm)
            .WithMessage($"Height must be between {UserWeightLog.Rules.MinHeightCm}cm and {UserWeightLog.Rules.MaxHeightCm}cm.");

        RuleFor(x => x.Notes)
            .MaximumLength(UserWeightLog.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.MusclePercent)
            .InclusiveBetween(UserWeightLog.Rules.MinPercent, UserWeightLog.Rules.MaxPercent)
            .When(x => x.MusclePercent.HasValue)
            .WithMessage($"Muscle percentage must be between {UserWeightLog.Rules.MinPercent}% and {UserWeightLog.Rules.MaxPercent}%.");

        RuleFor(x => x.BonePercent)
            .InclusiveBetween(UserWeightLog.Rules.MinPercent, UserWeightLog.Rules.MaxPercent)
            .When(x => x.BonePercent.HasValue)
            .WithMessage($"Bone percentage must be between {UserWeightLog.Rules.MinPercent}% and {UserWeightLog.Rules.MaxPercent}%.");

        RuleFor(x => x.FatPercent)
            .InclusiveBetween(UserWeightLog.Rules.MinPercent, UserWeightLog.Rules.MaxPercent)
            .When(x => x.FatPercent.HasValue)
            .WithMessage($"Fat percentage must be between {UserWeightLog.Rules.MinPercent}% and {UserWeightLog.Rules.MaxPercent}%.");

        RuleFor(x => x)
            .Must(x => (x.MusclePercent ?? 0.0) + (x.BonePercent ?? 0.0) + (x.FatPercent ?? 0.0) <= 100.0)
            .WithMessage("Total body composition percentage (Muscle + Bone + Fat) cannot exceed 100%.");
    }
}

public class LogUserWeightCommandHandler : ICommandHandler<LogUserWeightCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;

    public LogUserWeightCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(LogUserWeightCommand request, CancellationToken ct = default)
    {
        var log = UserWeightLog.Create(
            request.UserId,
            request.WeightKg,
            request.HeightCm,
            request.Date,
            request.Notes,
            request.MusclePercent,
            request.BonePercent,
            request.FatPercent);
        await _uow.UserWeightLogs.AddAsync(log);
        return Result.Success(log.Id);
    }
}

// ── Delete Weight Command ──────────────────────────────────────────────────────

public record DeleteUserWeightCommand(Guid UserId, Guid LogId) : ICommand<Result>;

public class DeleteUserWeightCommandHandler : ICommandHandler<DeleteUserWeightCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public DeleteUserWeightCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(DeleteUserWeightCommand request, CancellationToken ct = default)
    {
        var log = await _uow.UserWeightLogs.GetByIdAsync(request.LogId, ct);
        if (log == null) return Result.Failure("Weight log not found.");
        if (log.UserId != request.UserId) return Result.Failure("Not authorized to delete this log.");

        await _uow.UserWeightLogs.RemoveAsync(request.LogId, ct);
        return Result.Success();
    }
}

// ── Workout Analytics Query ───────────────────────────────────────────────────

public record GetWorkoutAnalyticsQuery(Guid UserId) : IQuery<Result<WorkoutAnalyticsDto>>;

public class GetWorkoutAnalyticsQueryHandler : BaseQueryHandler<GetWorkoutAnalyticsQuery, Result<WorkoutAnalyticsDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetWorkoutAnalyticsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetWorkoutAnalyticsQueryHandler> logger)
        : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<WorkoutAnalyticsDto>> ExecuteAsync(GetWorkoutAnalyticsQuery request, CancellationToken ct)
    {
        var sessionsAll = await _uow.UserTrainingSessions.GetAllAsync();
        var sessions = sessionsAll.Where(s => s.UserId == request.UserId).OrderBy(s => s.StartTime).ToList();

        var weightsAll = await _uow.UserWeightLogs.GetAllAsync();
        var weights = weightsAll.Where(w => w.UserId == request.UserId).OrderBy(w => w.Date).ToList();

        var trainingsAll = await _uow.Trainings.GetAllAsync();
        var trainingsDict = trainingsAll.ToDictionary(t => t.Id, t => t.Name);

        // Stats from sessions
        int totalSessions = sessions.Count;
        int totalDurationMinutes = sessions.Sum(s => s.TotalDurationSeconds) / 60;
        double totalCalories = Math.Round(sessions.Sum(s => s.TotalCaloriesBurned), 1);

        var allLogs = sessions.SelectMany(s => s.Logs).ToList();
        int totalExercisesLogged = allLogs.Count;
        double totalVolumeLifted = Math.Round(allLogs.Sum(l => l.SetsCompleted * l.RepsCompleted * l.WeightUsed), 1);

        // Exercise statistics
        var topExercises = allLogs
            .GroupBy(l => l.TrainingId)
            .Select(g => new ExerciseStatDto(
                g.Key,
                trainingsDict.TryGetValue(g.Key, out var name) ? name : "Exercise",
                g.Sum(x => x.SetsCompleted),
                g.Sum(x => x.RepsCompleted),
                g.Max(x => x.WeightUsed),
                Math.Round(g.Sum(x => x.SetsCompleted * x.RepsCompleted * x.WeightUsed), 1),
                g.Count()))
            .OrderByDescending(e => e.TotalVolume)
            .ThenByDescending(e => e.MaxWeight)
            .Take(10)
            .ToList();

        // Session Trend by Month
        var sessionTrend = sessions
            .GroupBy(s => new { s.StartTime.Year, s.StartTime.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new WorkoutSessionPointDto(
                $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                g.Count(),
                g.Sum(x => x.TotalDurationSeconds) / 60))
            .ToList();

        // Weight Logs DTOs
        var weightDtos = weights
            .OrderByDescending(w => w.Date)
            .Select(w => new UserWeightLogDto(
                w.Id,
                w.UserId,
                w.WeightKg,
                w.HeightCm,
                w.Bmi,
                w.BmiCategory,
                w.Date,
                w.Notes,
                w.MusclePercent,
                w.BonePercent,
                w.FatPercent))
            .ToList();

        double? startingWeight = weights.FirstOrDefault()?.WeightKg;
        double? currentWeight = weights.LastOrDefault()?.WeightKg;
        double? weightChange = (currentWeight.HasValue && startingWeight.HasValue)
            ? Math.Round(currentWeight.Value - startingWeight.Value, 1)
            : null;
        double? currentHeight = weights.LastOrDefault()?.HeightCm;
        double? currentBmi = weights.LastOrDefault()?.Bmi;
        string? currentBmiCategory = weights.LastOrDefault()?.BmiCategory;

        // Body Composition Metrics (from latest log that recorded body composition)
        var latestWithComposition = weights.LastOrDefault(w => w.MusclePercent.HasValue || w.FatPercent.HasValue || w.BonePercent.HasValue);
        double? currentMuscle = latestWithComposition?.MusclePercent;
        double? currentBone = latestWithComposition?.BonePercent;
        double? currentFat = latestWithComposition?.FatPercent;
        double? currentOther = (currentMuscle.HasValue || currentBone.HasValue || currentFat.HasValue)
            ? Math.Max(0.0, Math.Round(100.0 - (currentMuscle ?? 0.0) - (currentBone ?? 0.0) - (currentFat ?? 0.0), 1))
            : null;

        double? fatMassKg = (currentWeight.HasValue && currentFat.HasValue)
            ? Math.Round(currentWeight.Value * (currentFat.Value / 100.0), 1)
            : null;
        double? leanMassKg = (currentWeight.HasValue && fatMassKg.HasValue)
            ? Math.Round(currentWeight.Value - fatMassKg.Value, 1)
            : null;
        double? muscleMassKg = (currentWeight.HasValue && currentMuscle.HasValue)
            ? Math.Round(currentWeight.Value * (currentMuscle.Value / 100.0), 1)
            : null;

        var firstWithComposition = weights.FirstOrDefault(w => w.MusclePercent.HasValue || w.FatPercent.HasValue);
        double? muscleChange = (currentMuscle.HasValue && firstWithComposition?.MusclePercent.HasValue == true && latestWithComposition != firstWithComposition)
            ? Math.Round(currentMuscle.Value - firstWithComposition.MusclePercent.Value, 1)
            : null;
        double? fatChange = (currentFat.HasValue && firstWithComposition?.FatPercent.HasValue == true && latestWithComposition != firstWithComposition)
            ? Math.Round(currentFat.Value - firstWithComposition.FatPercent.Value, 1)
            : null;

        var dto = new WorkoutAnalyticsDto(
            totalSessions,
            totalDurationMinutes,
            totalCalories,
            totalExercisesLogged,
            totalVolumeLifted,
            currentWeight,
            startingWeight,
            weightChange,
            currentHeight,
            currentBmi,
            currentBmiCategory,
            weightDtos,
            topExercises,
            sessionTrend,
            currentMuscle,
            currentBone,
            currentFat,
            currentOther,
            leanMassKg,
            fatMassKg,
            muscleMassKg,
            muscleChange,
            fatChange);

        return Result.Success(dto);
    }
}
