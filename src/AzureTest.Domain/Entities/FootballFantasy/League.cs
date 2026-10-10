using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class League : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;
        public const int ShortNameMaxLength = 50;
        public const int ContinentMaxLength = 50;
        public const int LogoUrlMaxLength = 500;
    }

    public string Name { get; private set; } = default!;
    public string? ShortName { get; private set; }
    public LeagueType Type { get; private set; }
    public LeagueLevel Level { get; private set; }
    public Guid? CountryId { get; private set; }
    public Country? Country { get; private set; }
    public string? Continent { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Season> Seasons { get; private set; } = new List<Season>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private League() { }

    public static League Create(
        string name,
        LeagueType type,
        LeagueLevel level,
        string? shortName = null,
        Guid? countryId = null,
        string? continent = null,
        string? logoUrl = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid league name.", nameof(name));

        return new League
        {
            Name = name.Trim(),
            ShortName = shortName?.Trim(),
            Type = type,
            Level = level,
            CountryId = countryId,
            Continent = continent?.Trim(),
            LogoUrl = logoUrl?.Trim(),
            IsActive = isActive
        };
    }

    public void Update(
        string name,
        string? shortName,
        LeagueType type,
        LeagueLevel level,
        Guid? countryId,
        string? continent,
        string? logoUrl,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid league name.", nameof(name));

        Name = name.Trim();
        ShortName = shortName?.Trim();
        Type = type;
        Level = level;
        CountryId = countryId;
        Continent = continent?.Trim();
        LogoUrl = logoUrl?.Trim();
        IsActive = isActive;
    }
}
