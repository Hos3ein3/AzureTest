using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class TaskItem : AggregateRoot, ISoftDelete, IAudit
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? Priority { get; private set; }
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

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, DateTime? dueDate, string? priority, Guid userId)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority,
            IsCompleted = false,
            UserId = userId
        };
    }

    public void Update(string title, string? description, DateTime? dueDate, string? priority)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
    }

    public void ToggleCompletion()
    {
        IsCompleted = !IsCompleted;
    }
}
