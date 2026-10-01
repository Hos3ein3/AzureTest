using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.SampleNotes;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────
public sealed record GetSampleNoteByIdQuery(Guid Id) : IQuery<SampleNoteDto?>;

// ── Handler + Mapper ──────────────────────────────────────────────────────────
public sealed class GetSampleNoteByIdQueryHandler : BaseQueryHandler<GetSampleNoteByIdQuery, SampleNoteDto?>
{
    private readonly ISampleNoteStore _store;

    public GetSampleNoteByIdQueryHandler(ISampleNoteStore store, ILogger<GetSampleNoteByIdQueryHandler> logger)
        : base(logger)
    {
        _store = store;
    }

    protected override Task<SampleNoteDto?> ExecuteAsync(GetSampleNoteByIdQuery query, CancellationToken cancellationToken)
    {
        var note = _store.GetById(query.Id);
        return Task.FromResult(note is null ? null : MapToDto(note));
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static SampleNoteDto MapToDto(SampleNote n) => new(
        n.Id, n.Title, n.Content, n.Category, n.Priority, n.IsPinned, n.CreatedAt, n.UpdatedAt);
}
