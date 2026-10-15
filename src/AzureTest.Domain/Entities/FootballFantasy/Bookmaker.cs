using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Bookmaker : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int UrlMaxLength = 500;
    }

    public string Name { get; private set; } = default!;
    public string? Url { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<FixtureOdd> Odds { get; private set; } = new List<FixtureOdd>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Bookmaker() { }

    public static Bookmaker Create(string name, string? url = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid bookmaker name.", nameof(name));

        return new Bookmaker
        {
            Name = name.Trim(),
            Url = url?.Trim(),
            IsActive = isActive
        };
    }

    public void Update(string name, string? url = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid bookmaker name.", nameof(name));

        Name = name.Trim();
        Url = url?.Trim();
        IsActive = isActive;
    }
}
