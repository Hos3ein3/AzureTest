using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Commands;

// ── Command ───────────────────────────────────────────────────────────────────
public sealed record DeleteSampleNoteCommand(Guid Id) : ICommand<bool>;

// ── Handler ───────────────────────────────────────────────────────────────────
public sealed class DeleteSampleNoteCommandHandler : ICommandHandler<DeleteSampleNoteCommand, bool>
{
    private readonly ISampleNoteStore _store;
    private readonly ILogger<DeleteSampleNoteCommandHandler> _logger;

    public DeleteSampleNoteCommandHandler(ISampleNoteStore store, ILogger<DeleteSampleNoteCommandHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task<bool> HandleAsync(DeleteSampleNoteCommand command, CancellationToken cancellationToken = default)
    {
        var deleted = _store.Delete(command.Id);
        if (deleted)
            _logger.LogInformation("SampleNote {NoteId} deleted", command.Id);
        else
            _logger.LogWarning("SampleNote {NoteId} not found for deletion", command.Id);

        return Task.FromResult(deleted);
    }
}
