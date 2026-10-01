using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.SampleNotes;

public sealed record SampleNoteDto(
    Guid Id,
    string Title,
    string Content,
    NoteCategory Category,
    int Priority,
    bool IsPinned,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
