using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Commands;

// ── Command ───────────────────────────────────────────────────────────────────
public sealed record DeleteSampleNoteCommand(Guid Id) : ICommand<Result>;

// ── Handler ───────────────────────────────────────────────────────────────────
public sealed class DeleteSampleNoteCommandHandler : ICommandHandler<DeleteSampleNoteCommand, Result>
{
    private readonly ISampleNoteStore _store;
    private readonly ILogger<DeleteSampleNoteCommandHandler> _logger;

    public DeleteSampleNoteCommandHandler(ISampleNoteStore store, ILogger<DeleteSampleNoteCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<Result> HandleAsync(DeleteSampleNoteCommand command, CancellationToken cancellationToken = default)
    {
        var deleted = _store.Delete(command.Id);
        if (deleted)
        {
            _logger.LogInformation("SampleNote {NoteId} deleted", command.Id);
            return Task.FromResult(Result.Success());
        }
        
            _logger.LogWarning("SampleNote {NoteId} not found for deletion", command.Id);
            return Task.FromResult(Result.Failure());
    }
}
