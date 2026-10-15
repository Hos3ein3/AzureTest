using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreateFantasyLeagueCommand(
    string Name,
    string? Description,
    FantasyLeagueVisibility Visibility,
    FantasyLeagueMode Mode = FantasyLeagueMode.FreeForAll,
    FantasyPeriodType PeriodType = FantasyPeriodType.Weekly,
    FantasySquadMode SquadMode = FantasySquadMode.SeasonSquad,
    int SquadSize = 15,
    int StartingSize = 11,
    int? MinSquadSize = 11,
    decimal Budget = 100.0m,
    string? AllowedFormations = null,
    string? JoinCode = null,
    DateTime StartsAt = default,
    DateTime EndsAt = default,
    int? MaxMembers = null,
    string? CustomTag = null,
    string? LogoUrl = null,
    bool AllowCoach = true,
    FantasyKickoutType KickoutType = FantasyKickoutType.FullRemoval,
    DateTime? AuctionStartsAt = null,
    DateTime? AuctionEndsAt = null,
    FantasyLeagueFormat Format = FantasyLeagueFormat.TotalPoints,
    bool AllowAutoSub = false,
    bool EnableMvpBonus = false,
    int MvpBonusPoints = 3,
    int? GroupCount = null,
    // Player Diversity / Scope
    PoolScopeType ScopeType = PoolScopeType.All,
    string? ScopeValue = null,
    // Transfer Market Windows (e.g. Summer & Winter, or Single event window)
    DateTime? Window1Open = null,
    DateTime? Window1Close = null,
    DateTime? Window2Open = null,
    DateTime? Window2Close = null,
    // Advanced Transfer Rules & Taxes
    int FreeTransfersPerGameweek = 1,
    int? ExtraTransferPointPenalty = 4,
    decimal ProfitTaxPercent = 50.0m,
    // Scoring Rules Configuration
    int ScoringAppearance = 2,
    int ScoringPlayed60Plus = 1,
    int ScoringGoalForward = 4,
    int ScoringGoalMidfielder = 5,
    int ScoringGoalDefender = 6,
    int ScoringAssist = 3,
    int ScoringCleanSheet = 4,
    int ScoringPenaltySaved = 5,
    int ScoringPenaltyMissed = -2,
    int ScoringYellowCard = -1,
    int ScoringRedCard = -3,
    int ScoringOwnGoal = -2
) : ICommand<Result<Guid>>;

public sealed class CreateFantasyLeagueCommandValidator : AbstractValidator<CreateFantasyLeagueCommand>
{
    public CreateFantasyLeagueCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("League name is required.")
            .MinimumLength(FantasyLeague.Rules.NameMinLength)
            .MaximumLength(FantasyLeague.Rules.NameMaxLength);

        RuleFor(x => x.MaxMembers)
            .InclusiveBetween(FantasyLeague.Rules.MinLeagueMembers, FantasyLeague.Rules.MaxLeagueMembers)
            .When(x => x.Visibility != FantasyLeagueVisibility.Global && x.MaxMembers.HasValue)
            .WithMessage($"Maximum teams must be between {FantasyLeague.Rules.MinLeagueMembers} and {FantasyLeague.Rules.MaxLeagueMembers}.");

        RuleFor(x => x.CustomTag)
            .Matches(FantasyLeague.Rules.LeagueTagPattern)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomTag))
            .WithMessage($"Tag must be 10-12 uppercase letters and numbers.");
    }
}

public sealed class CreateFantasyLeagueCommandHandler : BaseCommandHandler<CreateFantasyLeagueCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;

    public CreateFantasyLeagueCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<CreateFantasyLeagueCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateFantasyLeagueCommand cmd, CancellationToken cancellationToken)
    {
        // 1. Only admin can create Global leagues
        if (cmd.Visibility == FantasyLeagueVisibility.Global && !_currentUser.IsAdmin)
        {
            return Result<Guid>.Failure("Only system administrators can create Global fantasy leagues.");
        }

        // 2. Max members: Global leagues have NO player/member limit (null).
        // For public and private leagues, enforce bounds between 4 and 48.
        int? maxMembers = null;
        if (cmd.Visibility != FantasyLeagueVisibility.Global)
        {
            maxMembers = cmd.MaxMembers ?? FantasyLeague.Rules.MaxLeagueMembers;
            if (maxMembers < FantasyLeague.Rules.MinLeagueMembers || maxMembers > FantasyLeague.Rules.MaxLeagueMembers)
            {
                return Result<Guid>.Failure($"League capacity must be between {FantasyLeague.Rules.MinLeagueMembers} and {FantasyLeague.Rules.MaxLeagueMembers} teams.");
            }
        }

        // 3. Auto-generate unique 10-12 uppercase alphanumeric Tag (or validate custom tag if provided by admin)
        string tag;
        if (!string.IsNullOrWhiteSpace(cmd.CustomTag) && _currentUser.IsAdmin)
        {
            tag = cmd.CustomTag.Trim().ToUpperInvariant();
            var tagCheck = await UnitOfWork.FantasyLeagues.FindAsync(l => l.LeagueTag == tag);
            if (tagCheck.Count > 0) return Result<Guid>.Failure("Specified league tag already exists.");
        }
        else
        {
            tag = await GenerateUniqueLeagueTagAsync();
        }

        // 4. Private league must have join/invite code
        string? joinCode = cmd.JoinCode?.Trim();
        if (cmd.Visibility == FantasyLeagueVisibility.Private && string.IsNullOrWhiteSpace(joinCode))
        {
            // Auto generate private invite code if none provided
            joinCode = GenerateRandomAlphaNumeric(8);
        }

        var isGlobal = cmd.Visibility == FantasyLeagueVisibility.Global;

        // 5. Create a dedicated ScoringRuleSet directly for this league
        var ruleSetName = $"{cmd.Name.Trim()} Scoring Rules";
        var ruleSet = ScoringRuleSet.Create(ruleSetName, isGlobal ? null : _currentUser.Id, false);
        UnitOfWork.ScoringRuleSets.Add(ruleSet);

        // Add customizable ScoringRules
        var rules = new List<ScoringRule>
        {
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Appearance, cmd.ScoringAppearance),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Played60Plus, cmd.ScoringPlayed60Plus),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Goal, cmd.ScoringGoalForward, PlayerPosition.Forward),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Goal, cmd.ScoringGoalMidfielder, PlayerPosition.Midfielder),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Goal, cmd.ScoringGoalDefender, PlayerPosition.Defender),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Goal, cmd.ScoringGoalDefender, PlayerPosition.Goalkeeper),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.Assist, cmd.ScoringAssist),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.CleanSheet, cmd.ScoringCleanSheet, PlayerPosition.Goalkeeper),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.CleanSheet, cmd.ScoringCleanSheet, PlayerPosition.Defender),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.PenaltySaved, cmd.ScoringPenaltySaved, PlayerPosition.Goalkeeper),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.PenaltyMissed, cmd.ScoringPenaltyMissed),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.YellowCard, cmd.ScoringYellowCard),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.RedCard, cmd.ScoringRedCard),
            ScoringRule.Create(ruleSet.Id, ScoringMetric.OwnGoal, cmd.ScoringOwnGoal)
        };

        foreach (var rule in rules)
        {
            UnitOfWork.ScoringRules.Add(rule);
        }

        // 6. Create Fantasy League
        var startsAt = (cmd.StartsAt == default ? DateTime.UtcNow : cmd.StartsAt).ToUtc();
        var endsAt = (cmd.EndsAt == default ? DateTime.UtcNow.AddMonths(9) : cmd.EndsAt).ToUtc();

        var league = FantasyLeague.Create(
            cmd.Name,
            tag,
            cmd.Visibility,
            cmd.Mode,
            cmd.PeriodType,
            cmd.SquadMode,
            cmd.SquadSize <= 0 ? FantasyLeague.Rules.DefaultSquadSize : cmd.SquadSize,
            cmd.StartingSize <= 0 ? FantasyLeague.Rules.DefaultStartingSize : cmd.StartingSize,
            cmd.Budget <= 0 ? FantasyLeague.Rules.DefaultBudget : cmd.Budget,
            ruleSet.Id,
            startsAt,
            endsAt,
            isGlobal ? null : _currentUser.Id,
            cmd.Description,
            allowedFormations: cmd.AllowedFormations ?? FantasyLeague.Rules.DefaultAllowedFormations,
            allowCoach: cmd.AllowCoach,
            maxMembers: maxMembers,
            joinCode: joinCode,
            logoUrl: cmd.LogoUrl,
            window1Open: cmd.Window1Open.ToUtc(),
            window1Close: cmd.Window1Close.ToUtc(),
            window2Open: cmd.Window2Open.ToUtc(),
            window2Close: cmd.Window2Close.ToUtc(),
            minSquadSize: cmd.MinSquadSize,
            kickoutType: cmd.KickoutType,
            auctionStartsAt: cmd.AuctionStartsAt.ToUtc(),
            auctionEndsAt: cmd.AuctionEndsAt.ToUtc(),
            format: cmd.Format,
            allowAutoSub: cmd.AllowAutoSub,
            enableMvpBonus: cmd.EnableMvpBonus,
            mvpBonusPoints: cmd.MvpBonusPoints,
            groupCount: cmd.GroupCount,
            freeTransfersPerGameweek: cmd.FreeTransfersPerGameweek,
            extraTransferPointPenalty: cmd.ExtraTransferPointPenalty,
            profitTaxPercent: cmd.ProfitTaxPercent
        );

        UnitOfWork.FantasyLeagues.Add(league);

        // 7. Add Player Diversity / Pool Scope Rule
        var scopeValue = !string.IsNullOrWhiteSpace(cmd.ScopeValue) ? cmd.ScopeValue.Trim() : "All";
        var poolRule = FantasyLeaguePoolRule.Create(league.Id, cmd.ScopeType, scopeValue, false);
        UnitOfWork.FantasyLeaguePoolRules.Add(poolRule);

        if (!isGlobal && _currentUser.Id != Guid.Empty)
        {
            var member = FantasyLeagueMember.Create(league.Id, _currentUser.Id, FantasyMemberRole.Admin);
            UnitOfWork.FantasyLeagueMembers.Add(member);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(league.Id);
    }

    private async Task<string> GenerateUniqueLeagueTagAsync()
    {
        while (true)
        {
            // Combination of 10-12 numbers and uppercase characters
            var length = RandomNumberGenerator.GetInt32(FantasyLeague.Rules.LeagueTagMinLength, FantasyLeague.Rules.LeagueTagMaxLength + 1);
            var tag = GenerateRandomAlphaNumeric(length);

            var existing = await UnitOfWork.FantasyLeagues.FindAsync(l => l.LeagueTag == tag);
            if (existing.Count == 0)
            {
                return tag;
            }
        }
    }

    private static string GenerateRandomAlphaNumeric(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // readable uppercase + digits (omitting O/0/I/1 for clarity)
        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(buffer);
    }
}
