using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.SampleNotes;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Commands;

// ── Command ───────────────────────────────────────────────────────────────────
public sealed record UpdateSampleNoteCommand(UpdateSampleNoteDto Dto) : ICommand<bool>;

// ── Validator ─────────────────────────────────────────────────────────────────
public sealed class UpdateSampleNoteCommandValidator : AbstractValidator<UpdateSampleNoteCommand>
{
    public UpdateSampleNoteCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Note data is required.");
        RuleFor(x => x.Dto.Id).NotEmpty().WithMessage("Note ID is required.");
        RuleFor(x => x.Dto.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(100);
        RuleFor(x => x.Dto.Priority).InclusiveBetween(1, 5).WithMessage("Priority must be 1–5.");
    }
}

// ── Handler + Mapper ──────────────────────────────────────────────────────────
public sealed class UpdateSampleNoteCommandHandler : ICommandHandler<UpdateSampleNoteCommand, bool>
{
    private readonly ISampleNoteStore _store;
    private readonly ILogger<UpdateSampleNoteCommandHandler> _logger;

    public UpdateSampleNoteCommandHandler(ISampleNoteStore store, ILogger<UpdateSampleNoteCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<bool> HandleAsync(UpdateSampleNoteCommand command, CancellationToken cancellationToken = default)
    {
        var existing = _store.GetById(command.Dto.Id);
        if (existing is null)
        {
            _logger.LogWarning("SampleNote {NoteId} not found for update", command.Dto.Id);
            return Task.FromResult(false);
        }

        ApplyUpdate(existing, command.Dto);
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = _store.Update(existing);
        _logger.LogInformation("SampleNote {NoteId} updated", command.Dto.Id);
        return Task.FromResult(updated);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static void ApplyUpdate(SampleNote note, UpdateSampleNoteDto dto)
    {
        note.Title = dto.Title;
        note.Content = dto.Content;
        note.Category = dto.Category;
        note.Priority = dto.Priority;
        note.IsPinned = dto.IsPinned;
    }
}
