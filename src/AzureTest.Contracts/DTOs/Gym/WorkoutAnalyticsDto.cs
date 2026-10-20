using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Gym;

public sealed record UserWeightLogDto(
    Guid Id,
    Guid UserId,
    double WeightKg,
    double HeightCm,
    double Bmi,
    string BmiCategory,
    DateTime Date,
    string? Notes);

public sealed class LogWeightRequest
{
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
}

public sealed record ExerciseStatDto(
    Guid TrainingId,
    string TrainingName,
    int TotalSets,
    int TotalReps,
    double MaxWeight,
    double TotalVolume,
    int SessionCount);

public sealed record WorkoutSessionPointDto(
    string DateLabel,
    int SessionCount,
    int TotalMinutes);

public sealed record WorkoutAnalyticsDto(
    int TotalSessions,
    int TotalDurationMinutes,
    double TotalCaloriesBurned,
    int TotalExercisesLogged,
    double TotalVolumeLiftedKg,
    double? CurrentWeightKg,
    double? StartingWeightKg,
    double? WeightChangeKg,
    double? CurrentHeightCm,
    double? CurrentBmi,
    string? CurrentBmiCategory,
    IReadOnlyList<UserWeightLogDto> WeightLogs,
    IReadOnlyList<ExerciseStatDto> TopExercises,
    IReadOnlyList<WorkoutSessionPointDto> SessionTrend);
