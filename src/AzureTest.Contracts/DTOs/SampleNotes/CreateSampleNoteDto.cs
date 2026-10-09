using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.SampleNotes;

public sealed class CreateSampleNoteDto : ISampleNoteFormDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoteCategory Category { get; set; } = NoteCategory.General;
    public int Priority { get; set; } = 1;
    public bool IsPinned { get; set; }
}
