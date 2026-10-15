using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class BetType : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMaxLength = 100;
        public const int CodeMaxLength = 50;
        public const int DescriptionMaxLength = 250;
    }

    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private BetType() { }

    public static BetType Create(string name, string code, string? description = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));

        return new BetType
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            IsActive = isActive
        };
    }

    public void Update(string name, string code, string? description = null, bool isActive = true)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description?.Trim();
        IsActive = isActive;
    }
}
