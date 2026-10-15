using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Venue : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;
        public const int CityMaxLength = 100;
        public const int AddressMaxLength = 250;
        public const int SurfaceMaxLength = 50;
        public const int ImageUrlMaxLength = 500;
        public const int CapacityMin = 0;
    }

    public string Name { get; private set; } = default!;
    public string? City { get; private set; }
    public string? Address { get; private set; }
    public int? Capacity { get; private set; }
    public string? Surface { get; private set; }
    public string? ImageUrl { get; private set; }

    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = default!;

    public ICollection<Team> Teams { get; private set; } = new List<Team>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Venue() { }

    public static Venue Create(
        string name,
        Guid countryId,
        string? city = null,
        string? address = null,
        int? capacity = null,
        string? surface = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid venue name.", nameof(name));
        if (countryId == Guid.Empty)
            throw new ArgumentException("CountryId is required.", nameof(countryId));

        return new Venue
        {
            Name = name.Trim(),
            CountryId = countryId,
            City = city?.Trim(),
            Address = address?.Trim(),
            Capacity = capacity,
            Surface = surface?.Trim(),
            ImageUrl = imageUrl?.Trim()
        };
    }

    public void Update(
        string name,
        Guid countryId,
        string? city = null,
        string? address = null,
        int? capacity = null,
        string? surface = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid venue name.", nameof(name));
        if (countryId == Guid.Empty)
            throw new ArgumentException("CountryId is required.", nameof(countryId));

        Name = name.Trim();
        CountryId = countryId;
        City = city?.Trim();
        Address = address?.Trim();
        Capacity = capacity;
        Surface = surface?.Trim();
        ImageUrl = imageUrl?.Trim();
    }
}
