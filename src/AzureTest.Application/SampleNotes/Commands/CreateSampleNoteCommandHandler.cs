using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.SampleNotes;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Commands;

// ── Command ───────────────────────────────────────────────────────────────────
public sealed record CreateSampleNoteCommand(CreateSampleNoteDto Dto) : ICommand<Guid>;

// ── Validator ─────────────────────────────────────────────────────────────────
public sealed class CreateSampleNoteCommandValidator : AbstractValidator<CreateSampleNoteCommand>
{
    public CreateSampleNoteCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Note data is required.");
        RuleFor(x => x.Dto.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Dto.Priority).InclusiveBetween(1, 5).WithMessage("Priority must be 1–5.");
    }
}

// ── Handler + Mapper ──────────────────────────────────────────────────────────
public sealed class CreateSampleNoteCommandHandler : ICommandHandler<CreateSampleNoteCommand, Guid>
{
    private readonly ISampleNoteStore _store;
    private readonly ILogger<CreateSampleNoteCommandHandler> _logger;

    public CreateSampleNoteCommandHandler(ISampleNoteStore store, ILogger<CreateSampleNoteCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<Guid> HandleAsync(CreateSampleNoteCommand command, CancellationToken cancellationToken = default)
    {
        var note = MapToEntity(command.Dto);
        _store.Add(note);
        _logger.LogInformation("SampleNote {NoteId} '{Title}' created", note.Id, note.Title);
        return Task.FromResult(note.Id);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static SampleNote MapToEntity(CreateSampleNoteDto dto) => new()
    {
        Title = dto.Title,
        Content = dto.Content,
        Category = dto.Category,
        Priority = dto.Priority,
        IsPinned = dto.IsPinned,
        CreatedAt = DateTime.UtcNow
    };
}
