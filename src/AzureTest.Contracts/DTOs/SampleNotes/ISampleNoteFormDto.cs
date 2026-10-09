using System;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.SampleNotes;

public interface ISampleNoteFormDto
{
    string Title { get; set; }
    string Content { get; set; }
    NoteCategory Category { get; set; }
    int Priority { get; set; }
    bool IsPinned { get; set; }
}
