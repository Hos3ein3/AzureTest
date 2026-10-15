using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Team : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;
        public const int ShortNameMaxLength = 50;
        public const int CodeMaxLength = 10;
        public const int StadiumNameMaxLength = 150;
        public const int LogoUrlMaxLength = 500;
        public const int FoundedYearMin = 1800;
        public const int FoundedYearMax = 2100;
        public const int StadiumCapacityMin = 0;
    }

    public string Name { get; private set; } = default!;
    public string ShortName { get; private set; } = default!;
    public string? Code { get; private set; }
    public TeamType TeamType { get; private set; }
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = default!;
    public Guid? VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public int? FoundedYear { get; private set; }
    public string? StadiumName { get; private set; }
    public int? StadiumCapacity { get; private set; }
    public string? LogoUrl { get; private set; }

    public ICollection<TeamSeason> TeamSeasons { get; private set; } = new List<TeamSeason>();
    public ICollection<TeamCoach> TeamCoaches { get; private set; } = new List<TeamCoach>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Team() { }

    public static Team Create(
        string name,
        string shortName,
        TeamType teamType,
        Guid countryId,
        string? code = null,
        int? foundedYear = null,
        string? stadiumName = null,
        int? stadiumCapacity = null,
        string? logoUrl = null,
        Guid? venueId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid team name.", nameof(name));
        if (string.IsNullOrWhiteSpace(shortName) || shortName.Length > Rules.ShortNameMaxLength)
            throw new ArgumentException("Invalid short name.", nameof(shortName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        return new Team
        {
            Name = name.Trim(),
            ShortName = shortName.Trim(),
            TeamType = teamType,
            CountryId = countryId,
            Code = code?.Trim().ToUpperInvariant(),
            FoundedYear = foundedYear,
            StadiumName = stadiumName?.Trim(),
            StadiumCapacity = stadiumCapacity,
            LogoUrl = logoUrl?.Trim(),
            VenueId = venueId
        };
    }

    public void Update(
        string name,
        string shortName,
        TeamType teamType,
        Guid countryId,
        string? code = null,
        int? foundedYear = null,
        string? stadiumName = null,
        int? stadiumCapacity = null,
        string? logoUrl = null,
        Guid? venueId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid team name.", nameof(name));
        if (string.IsNullOrWhiteSpace(shortName) || shortName.Length > Rules.ShortNameMaxLength)
            throw new ArgumentException("Invalid short name.", nameof(shortName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        Name = name.Trim();
        ShortName = shortName.Trim();
        TeamType = teamType;
        CountryId = countryId;
        Code = code?.Trim().ToUpperInvariant();
        FoundedYear = foundedYear;
        StadiumName = stadiumName?.Trim();
        StadiumCapacity = stadiumCapacity;
        LogoUrl = logoUrl?.Trim();
        VenueId = venueId;
    }
}

