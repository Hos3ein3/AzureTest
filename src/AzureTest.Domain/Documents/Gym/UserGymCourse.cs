using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class UserGymCourse {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid OriginalCourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CourseWeek> Weeks { get; set; } = new();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public List<Guid> CompletedDayIds { get; set; } = new();
}
