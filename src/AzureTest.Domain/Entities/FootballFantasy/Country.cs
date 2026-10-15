using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Country : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int CodeMinLength = 2;
        public const int CodeMaxLength = 10;
        public const int ContinentMaxLength = 50;
        public const int FlagUrlMaxLength = 500;
    }

    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string Continent { get; private set; } = default!;
    public string? FlagUrl { get; private set; }

    public ICollection<League> Leagues { get; private set; } = new List<League>();
    public ICollection<Team> Teams { get; private set; } = new List<Team>();
    public ICollection<Player> Players { get; private set; } = new List<Player>();
    public ICollection<Coach> Coaches { get; private set; } = new List<Coach>();
    public ICollection<Venue> Venues { get; private set; } = new List<Venue>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Country() { }

    public static Country Create(string name, string code, string continent, string? flagUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid country name.", nameof(name));
        if (string.IsNullOrWhiteSpace(code) || code.Length > Rules.CodeMaxLength)
            throw new ArgumentException("Invalid country code.", nameof(code));
        if (string.IsNullOrWhiteSpace(continent) || continent.Length > Rules.ContinentMaxLength)
            throw new ArgumentException("Invalid continent.", nameof(continent));

        return new Country
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Continent = continent.Trim(),
            FlagUrl = flagUrl?.Trim()
        };
    }

    public void Update(string name, string continent, string? flagUrl)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid country name.", nameof(name));
        if (string.IsNullOrWhiteSpace(continent) || continent.Length > Rules.ContinentMaxLength)
            throw new ArgumentException("Invalid continent.", nameof(continent));

        Name = name.Trim();
        Continent = continent.Trim();
        FlagUrl = flagUrl?.Trim();
    }
}
