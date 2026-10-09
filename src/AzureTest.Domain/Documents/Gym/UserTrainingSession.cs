using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class UserTrainingSession {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid UserGymCourseId { get; set; }
    public Guid CourseDayId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalDurationSeconds { get; set; }
    public double TotalCaloriesBurned { get; set; }
    public List<TrainingLog> Logs { get; set; } = new();
}
public class TrainingLog {
    public Guid TrainingId { get; set; }
    public int SetsCompleted { get; set; }
    public int RepsCompleted { get; set; }
    public int DurationCompletedSeconds { get; set; }
    public double WeightUsed { get; set; }
    public double CaloriesBurned { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
