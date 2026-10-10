using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Coach : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int PhotoUrlMaxLength = 500;
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public Guid CountryId { get; private set; }
    public Country Country { get; private set; } = default!;
    public DateTime? DateOfBirth { get; private set; }
    public string? PhotoUrl { get; private set; }

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

    private Coach() { }

    public static Coach Create(string firstName, string lastName, Guid countryId, DateTime? dateOfBirth = null, string? photoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > Rules.FirstNameMaxLength)
            throw new ArgumentException("Invalid first name.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > Rules.LastNameMaxLength)
            throw new ArgumentException("Invalid last name.", nameof(lastName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        return new Coach
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CountryId = countryId,
            DateOfBirth = dateOfBirth,
            PhotoUrl = photoUrl?.Trim()
        };
    }

    public void Update(string firstName, string lastName, Guid countryId, DateTime? dateOfBirth = null, string? photoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > Rules.FirstNameMaxLength)
            throw new ArgumentException("Invalid first name.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length > Rules.LastNameMaxLength)
            throw new ArgumentException("Invalid last name.", nameof(lastName));
        if (countryId == Guid.Empty) throw new ArgumentException("CountryId is required.", nameof(countryId));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CountryId = countryId;
        DateOfBirth = dateOfBirth;
        PhotoUrl = photoUrl?.Trim();
    }
}
