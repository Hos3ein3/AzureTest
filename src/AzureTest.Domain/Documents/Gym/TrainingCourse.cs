using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class TrainingCourse {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<CourseWeek> Weeks { get; set; } = new();
}
public class CourseWeek {
    public int WeekNumber { get; set; }
    public List<CourseDay> Days { get; set; } = new();
}
public class CourseDay {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Equipments { get; set; } = new();
    public List<CourseSection> Sections { get; set; } = new();
}
public class CourseSection {
    public string Name { get; set; } = string.Empty;
    public List<CourseTraining> Trainings { get; set; } = new();
}
public class CourseTraining {
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TrainingId { get; set; }
    public int Order { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double Weight { get; set; }
    public int DurationSeconds { get; set; }
    public int RestSeconds { get; set; }
}
