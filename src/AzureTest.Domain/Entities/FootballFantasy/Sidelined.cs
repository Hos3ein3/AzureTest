using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Sidelined : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int TypeMaxLength = 100;
        public const int DescriptionMaxLength = 250;
    }

    public Guid? PlayerId { get; private set; }
    public Player? Player { get; private set; }
    public Guid? CoachId { get; private set; }
    public Coach? Coach { get; private set; }

    public string Type { get; private set; } = default!; // e.g. "Suspension", "Red Card Ban", "Disciplinary"
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Sidelined() { }

    public static Sidelined Create(
        string type,
        DateTime startDate,
        DateTime? endDate = null,
        string? description = null,
        Guid? playerId = null,
        Guid? coachId = null)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.", nameof(type));

        return new Sidelined
        {
            Type = type.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            Description = description?.Trim(),
            PlayerId = playerId,
            CoachId = coachId
        };
    }

    public void Update(string type, DateTime startDate, DateTime? endDate = null, string? description = null)
    {
        Type = type.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Description = description?.Trim();
    }
}
