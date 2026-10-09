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
public class SampleNoteFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public string? Search
    {
        get => SearchTerm;
        set => SearchTerm = value;
    }
    public NoteCategory? Category { get; set; }
    public bool? IsPinned { get; set; }

    public SampleNoteFilter()
    {
        PageSize = 20;
    }

    public SampleNoteFilter(
        string? Search = null,
        NoteCategory? Category = null,
        bool? IsPinned = null,
        int PageNumber = 1,
        int PageSize = 20
    )
    {
        this.Search = Search;
        this.Category = Category;
        this.IsPinned = IsPinned;
        this.PageNumber = PageNumber;
        this.PageSize = PageSize;
    }
}

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

        var search = query.Filter.Search ?? query.Filter.SearchTerm;
        if (!string.IsNullOrWhiteSpace(search))
            notes = notes.Where(n =>
                n.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                n.Content.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (query.Filter.Category.HasValue)
            notes = notes.Where(n => n.Category == query.Filter.Category.Value);

        if (query.Filter.IsPinned.HasValue)
            notes = notes.Where(n => n.IsPinned == query.Filter.IsPinned.Value);

        var ordered = notes
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .Select(MapToDto);

        var result = (query.Filter.PageSize > 0)
            ? ordered.Skip((Math.Max(1, query.Filter.PageNumber) - 1) * query.Filter.PageSize).Take(query.Filter.PageSize).ToList()
            : ordered.ToList();

        return Task.FromResult(result);
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static SampleNoteDto MapToDto(SampleNote n) => new(
        n.Id, n.Title, n.Content, n.Category, n.Priority, n.IsPinned, n.CreatedAt, n.UpdatedAt);
}
