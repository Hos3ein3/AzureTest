

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class HabitLog : BaseEntity, IAudit, ISoftDelete
{
    public Guid HabitTrackerId { get; private set; }
    public DateTime LogDate { get; private set; }
    public bool BooleanValue { get; private set; }
    public double NumericValue { get; private set; }
    public string? Notes { get; private set; }

    public HabitTracker HabitTracker { get; private set; } = null!;

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private HabitLog() { }

    public static HabitLog Create(Guid habitTrackerId, DateTime logDate, bool booleanValue, double numericValue, string? notes)
    {
        return new HabitLog
        {
            Id = Guid.NewGuid(),
            HabitTrackerId = habitTrackerId,
            LogDate = logDate,
            BooleanValue = booleanValue,
            NumericValue = numericValue,
            Notes = notes
        };
    }

    public void Update(bool booleanValue, double numericValue, string? notes)
    {
        BooleanValue = booleanValue;
        NumericValue = numericValue;
        Notes = notes;
    }
}
