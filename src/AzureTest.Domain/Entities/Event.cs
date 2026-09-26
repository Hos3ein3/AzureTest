using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class Event : AggregateRoot, ISoftDelete, IAudit
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsAllDay { get; private set; }
    public string? Color { get; private set; }
    public Guid UserId { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Event() { }

    public static Event Create(string title, string? description, DateTime startDate, DateTime endDate, bool isAllDay, string? color, Guid userId)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            IsAllDay = isAllDay,
            Color = color,
            UserId = userId
        };
    }

    public void Update(string title, string? description, DateTime startDate, DateTime endDate, bool isAllDay, string? color)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        IsAllDay = isAllDay;
        Color = color;
    }
}
