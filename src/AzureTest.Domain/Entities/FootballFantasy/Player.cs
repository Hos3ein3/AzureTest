using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Player : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int DisplayNameMinLength = 2;
        public const int DisplayNameMaxLength = 150;
        public const int PhotoUrlMaxLength = 500;
        public const int HeightMin = 140;
        public const int HeightMax = 230;
        public const int WeightMin = 40;
        public const int WeightMax = 150;
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public DateTime? DateOfBirth { get; private set; }
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = default!;
    public PlayerPosition Position { get; private set; }
    public PreferredFoot? PreferredFoot { get; private set; }
    public int? HeightCm { get; private set; }
    public int? WeightKg { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<PlayerSquad> PlayerSquads { get; private set; } = new List<PlayerSquad>();
    public ICollection<PlayerMatchStat> MatchStats { get; private set; } = new List<PlayerMatchStat>();
    public ICollection<PlayerPrice> Prices { get; private set; } = new List<PlayerPrice>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Player() { }

    public static Player Create(
        string firstName,
        string lastName,
        string displayName,
        Guid countryId,
        PlayerPosition position,
        DateTime? dateOfBirth = null,
        PreferredFoot? preferredFoot = null,
        int? heightCm = null,
        int? weightKg = null,
        string? photoUrl = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > Rules.DisplayNameMaxLength)
            throw new ArgumentException("Invalid display name.", nameof(displayName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        return new Player
        {
            FirstName = firstName?.Trim() ?? string.Empty,
            LastName = lastName?.Trim() ?? string.Empty,
            DisplayName = displayName.Trim(),
            CountryId = countryId,
            Position = position,
            DateOfBirth = dateOfBirth,
            PreferredFoot = preferredFoot,
            HeightCm = heightCm,
            WeightKg = weightKg,
            PhotoUrl = photoUrl?.Trim(),
            IsActive = isActive
        };
    }

    public void Update(
        string firstName,
        string lastName,
        string displayName,
        Guid countryId,
        PlayerPosition position,
        DateTime? dateOfBirth = null,
        PreferredFoot? preferredFoot = null,
        int? heightCm = null,
        int? weightKg = null,
        string? photoUrl = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > Rules.DisplayNameMaxLength)
            throw new ArgumentException("Invalid display name.", nameof(displayName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        FirstName = firstName?.Trim() ?? string.Empty;
        LastName = lastName?.Trim() ?? string.Empty;
        DisplayName = displayName.Trim();
        CountryId = countryId;
        Position = position;
        DateOfBirth = dateOfBirth;
        PreferredFoot = preferredFoot;
        HeightCm = heightCm;
        WeightKg = weightKg;
        PhotoUrl = photoUrl?.Trim();
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
