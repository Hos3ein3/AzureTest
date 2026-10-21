using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.SaveCryptoJournal;

public sealed record SaveCryptoJournalCommand(
    Guid? Id,
    Guid? PositionId,
    Guid UserId,
    Guid TenantId,
    string Symbol,
    string StrategyTag,
    string EmotionTag,
    int Rating,
    string Notes,
    string LessonsLearned) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static CryptoJournalEntry ToDomain(SaveCryptoJournalCommand cmd) =>
            CryptoJournalEntry.Create(
                cmd.PositionId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.StrategyTag,
                cmd.EmotionTag,
                cmd.Rating,
                cmd.Notes,
                cmd.LessonsLearned);
    }
}

public sealed class SaveCryptoJournalCommandValidator : AbstractValidator<SaveCryptoJournalCommand>
{
    public SaveCryptoJournalCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MaximumLength(CryptoJournalEntry.Rules.SymbolMaxLength);

        RuleFor(x => x.Notes)
            .MaximumLength(CryptoJournalEntry.Rules.NotesMaxLength);

        RuleFor(x => x.LessonsLearned)
            .MaximumLength(CryptoJournalEntry.Rules.LessonsMaxLength);

        RuleFor(x => x.Rating)
            .InclusiveBetween(CryptoJournalEntry.Rules.RatingMin, CryptoJournalEntry.Rules.RatingMax);
    }
}

public sealed class SaveCryptoJournalCommandHandler : ICommandHandler<SaveCryptoJournalCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public SaveCryptoJournalCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(SaveCryptoJournalCommand cmd, CancellationToken ct)
    {
        if (cmd.Id.HasValue && cmd.Id.Value != Guid.Empty)
        {
            var existing = await _uow.CryptoJournalEntries.GetByIdAsync(cmd.Id.Value);
            if (existing != null)
            {
                existing.Update(cmd.StrategyTag, cmd.EmotionTag, cmd.Rating, cmd.Notes, cmd.LessonsLearned);
                await _uow.SaveChangesAsync(ct);
                return Result<Guid>.Success(existing.Id);
            }
        }

        var entry = SaveCryptoJournalCommand.Mapping.ToDomain(cmd);
        _uow.CryptoJournalEntries.Add(entry);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(entry.Id);
    }
}
