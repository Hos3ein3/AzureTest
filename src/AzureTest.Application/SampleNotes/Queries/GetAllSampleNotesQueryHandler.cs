using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.SampleNotes;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.SampleNotes.Queries;

// ── Filter ────────────────────────────────────────────────────────────────────
public sealed record SampleNoteFilter(
    string? Search = null,
    NoteCategory? Category = null,
    bool? IsPinned = null,
    int PageNumber = 1,
    int PageSize = 20
);

// ── Query ─────────────────────────────────────────────────────────────────────
public sealed record GetAllSampleNotesQuery(SampleNoteFilter Filter) : IQuery<List<SampleNoteDto>>;

// ── Handler + Mapper ──────────────────────────────────────────────────────────
public sealed class GetAllSampleNotesQueryHandler : BaseQueryHandler<GetAllSampleNotesQuery, List<SampleNoteDto>>
{
    private readonly ISampleNoteStore _store;

    public GetAllSampleNotesQueryHandler(ISampleNoteStore store, ILogger<GetAllSampleNotesQueryHandler> logger)
        : base(logger)
    {
        _store = store;
    }

    protected override Task<List<SampleNoteDto>> ExecuteAsync(GetAllSampleNotesQuery query, CancellationToken cancellationToken)
    {
        var notes = _store.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Filter.Search))
            notes = notes.Where(n =>
                n.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase) ||
                n.Content.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase));

        if (query.Filter.Category.HasValue)
            notes = notes.Where(n => n.Category == query.Filter.Category.Value);

        if (query.Filter.IsPinned.HasValue)
            notes = notes.Where(n => n.IsPinned == query.Filter.IsPinned.Value);

        var result = notes
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .Select(MapToDto)
            .ToList();

        return Task.FromResult(result);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static SampleNoteDto MapToDto(SampleNote n) => new(
        n.Id, n.Title, n.Content, n.Category, n.Priority, n.IsPinned, n.CreatedAt, n.UpdatedAt);
}
