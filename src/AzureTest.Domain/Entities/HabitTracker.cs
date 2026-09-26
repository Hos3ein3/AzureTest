using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class HabitTracker : AggregateRoot, ISoftDelete, IAudit
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public HabitType Type { get; private set; }
    public HabitFrequency Frequency { get; private set; }
    public double Goal { get; private set; }
    public Guid UserId { get; private set; }

    public ICollection<HabitLog> Logs { get; private set; } = new List<HabitLog>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private HabitTracker() { }

    public static HabitTracker Create(string name, string? description, HabitType type, HabitFrequency frequency, double goal, Guid userId)
    {
        return new HabitTracker
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Frequency = frequency,
            Goal = goal,
            UserId = userId
        };
    }

    public void Update(string name, string? description, HabitType type, HabitFrequency frequency, double goal)
    {
        Name = name;
        Description = description;
        Type = type;
        Frequency = frequency;
        Goal = goal;
    }
}
