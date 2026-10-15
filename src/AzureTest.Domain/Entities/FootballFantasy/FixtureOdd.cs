using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FixtureOdd : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int ValueLabelMaxLength = 50;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public Guid BookmakerId { get; private set; }
    public Bookmaker Bookmaker { get; private set; } = default!;

    public OddsMarketType MarketType { get; private set; }
    public string ValueLabel { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsLive { get; private set; }
    public DateTime RecordedAt { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FixtureOdd() { }

    public static FixtureOdd Create(
        Guid fixtureId,
        Guid bookmakerId,
        OddsMarketType marketType,
        string valueLabel,
        decimal price,
        bool isLive = false,
        DateTime? recordedAt = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (bookmakerId == Guid.Empty) throw new ArgumentException("BookmakerId is required.", nameof(bookmakerId));
        if (string.IsNullOrWhiteSpace(valueLabel)) throw new ArgumentException("Value label is required.", nameof(valueLabel));

        return new FixtureOdd
        {
            FixtureId = fixtureId,
            BookmakerId = bookmakerId,
            MarketType = marketType,
            ValueLabel = valueLabel.Trim(),
            Price = Math.Max(1.00m, price),
            IsLive = isLive,
            RecordedAt = recordedAt ?? DateTime.UtcNow
        };
    }

    public void Update(string valueLabel, decimal price, bool isLive)
    {
        ValueLabel = valueLabel.Trim();
        Price = Math.Max(1.00m, price);
        IsLive = isLive;
    }
}
