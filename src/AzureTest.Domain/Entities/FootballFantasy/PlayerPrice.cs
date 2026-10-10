using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerPrice : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public static readonly decimal MinPrice = 0.0m;
        public const int PricePrecision = 10;
        public const int PriceScale = 2;
    }

    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public decimal Price { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public PlayerPriceReason Reason { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerPrice() { }

    public static PlayerPrice Create(
        Guid playerId,
        Guid seasonId,
        decimal price,
        DateTime validFrom,
        PlayerPriceReason reason,
        DateTime? validTo = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (price < Rules.MinPrice) throw new ArgumentException("Price cannot be negative.", nameof(price));

        return new PlayerPrice
        {
            PlayerId = playerId,
            SeasonId = seasonId,
            Price = price,
            ValidFrom = validFrom,
            Reason = reason,
            ValidTo = validTo
        };
    }

    public void CloseValidity(DateTime validTo)
    {
        if (validTo < ValidFrom) throw new ArgumentException("ValidTo cannot precede ValidFrom.", nameof(validTo));
        ValidTo = validTo;
    }
}
