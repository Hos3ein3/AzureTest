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
    public HabitDirection Direction { get; private set; } = HabitDirection.AtLeast;
    public double Target { get; private set; }
    public double Goal => Target;
    public string? Schedule { get; private set; }
    public HabitFrequency Frequency { get; private set; } = HabitFrequency.Daily;
    public string TimeZone { get; private set; } = "UTC";
    public bool IsArchived { get; private set; }
    public bool IsPrivate { get; private set; }
    public string? TypeConfig { get; private set; }
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

    public static HabitTracker Create(
        string name,
        string? description,
        HabitType type,
        HabitDirection direction,
        double target,
        string? schedule,
        HabitFrequency frequency,
        string? timeZone,
        bool isPrivate,
        string? typeConfig,
        Guid userId)
    {
        return new HabitTracker
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Direction = direction,
            Target = target,
            Schedule = schedule ?? frequency.ToString(),
            Frequency = frequency,
            TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone,
            IsArchived = false,
            IsPrivate = isPrivate,
            TypeConfig = typeConfig,
            UserId = userId
        };
    }

    public static HabitTracker Create(string name, string? description, HabitType type, HabitFrequency frequency, double goal, Guid userId)
    {
        return Create(
            name: name,
            description: description,
            type: type,
            direction: HabitDirection.AtLeast,
            target: goal,
            schedule: frequency.ToString(),
            frequency: frequency,
            timeZone: "UTC",
            isPrivate: false,
            typeConfig: null,
            userId: userId
        );
    }

    public void Update(
        string name,
        string? description,
        HabitType type,
        HabitDirection direction,
        double target,
        string? schedule,
        HabitFrequency frequency,
        string? timeZone,
        bool isArchived,
        bool isPrivate,
        string? typeConfig)
    {
        Name = name;
        Description = description;
        Type = type;
        Direction = direction;
        Target = target;
        Schedule = schedule ?? frequency.ToString();
        Frequency = frequency;
        TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone;
        IsArchived = isArchived;
        IsPrivate = isPrivate;
        TypeConfig = typeConfig;
    }

    public void Update(string name, string? description, HabitType type, HabitFrequency frequency, double goal)
    {
        Name = name;
        Description = description;
        Type = type;
        Frequency = frequency;
        Schedule = frequency.ToString();
        Target = goal;
    }

    public void Archive() => IsArchived = true;
    public void Unarchive() => IsArchived = false;
}
