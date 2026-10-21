using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.SaveJournalEntry;

public sealed record SaveJournalEntryCommand(
    Guid? TradeId,
    string Symbol,
    DateTime TradeDateUtc,
    string MarketSession,
    string SetupType,
    string Emotions,
    string Mistakes,
    string LessonsLearned,
    string DetailedNotes,
    string? ScreenshotUrl,
    int ExecutionRating = 3,
    bool FollowedPlan = true
) : ICommand<Result<Guid>>;

public sealed class SaveJournalEntryCommandValidator : AbstractValidator<SaveJournalEntryCommand>
{
    public SaveJournalEntryCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MaximumLength(ForexJournalEntry.Rules.SymbolMaxLength);

        RuleFor(x => x.SetupType)
            .NotEmpty()
            .MaximumLength(ForexJournalEntry.Rules.SetupTypeMaxLength);

        RuleFor(x => x.MarketSession)
            .NotEmpty()
            .MaximumLength(ForexJournalEntry.Rules.SessionMaxLength);

        RuleFor(x => x.DetailedNotes)
            .MaximumLength(ForexJournalEntry.Rules.NotesMaxLength);

        RuleFor(x => x.ExecutionRating)
            .InclusiveBetween(ForexJournalEntry.Rules.RatingMin, ForexJournalEntry.Rules.RatingMax);
    }
}

public sealed class SaveJournalEntryCommandHandler : ICommandHandler<SaveJournalEntryCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public SaveJournalEntryCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> HandleAsync(SaveJournalEntryCommand command, CancellationToken cancellationToken)
    {
        var entry = ForexJournalEntry.Create(
            _currentUser.Id,
            command.TradeId,
            command.Symbol,
            command.TradeDateUtc,
            command.MarketSession,
            command.SetupType,
            command.Emotions,
            command.Mistakes,
            command.LessonsLearned,
            command.DetailedNotes,
            command.ScreenshotUrl,
            command.ExecutionRating,
            command.FollowedPlan
        );

        _unitOfWork.ForexJournalEntries.Add(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entry.Id);
    }
}
