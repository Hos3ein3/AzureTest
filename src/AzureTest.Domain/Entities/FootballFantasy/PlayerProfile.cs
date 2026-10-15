using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerProfile : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int BiographyMaxLength = 2000;
        public const int AgentMaxLength = 100;
        public const int MarketValueCurrencyMaxLength = 10;
    }

    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;

    public string? Biography { get; private set; }
    public string? Agent { get; private set; }
    public decimal? MarketValueEur { get; private set; }
    public DateTime? ContractUntil { get; private set; }
    public string? SocialMediaHandle { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerProfile() { }

    public static PlayerProfile Create(
        Guid playerId,
        string? biography = null,
        string? agent = null,
        decimal? marketValueEur = null,
        DateTime? contractUntil = null,
        string? socialMediaHandle = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));

        return new PlayerProfile
        {
            PlayerId = playerId,
            Biography = biography?.Trim(),
            Agent = agent?.Trim(),
            MarketValueEur = marketValueEur,
            ContractUntil = contractUntil,
            SocialMediaHandle = socialMediaHandle?.Trim()
        };
    }

    public void Update(
        string? biography = null,
        string? agent = null,
        decimal? marketValueEur = null,
        DateTime? contractUntil = null,
        string? socialMediaHandle = null)
    {
        Biography = biography?.Trim();
        Agent = agent?.Trim();
        MarketValueEur = marketValueEur;
        ContractUntil = contractUntil;
        SocialMediaHandle = socialMediaHandle?.Trim();
    }
}
