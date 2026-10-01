using AzureTest.Application.Messaging;
using AzureTest.Application.SampleNotes.Queries;
using AzureTest.Contracts.DTOs.SampleNotes;

namespace AzureTest.Application.SampleNotes;

public interface ISampleNoteQueryUnit
{
    IQueryHandler<GetAllSampleNotesQuery, List<SampleNoteDto>> GetAll { get; }
    IQueryHandler<GetSampleNoteByIdQuery, SampleNoteDto?> GetById { get; }
}

public sealed class SampleNoteQueryUnit : ISampleNoteQueryUnit
{
    public IQueryHandler<GetAllSampleNotesQuery, List<SampleNoteDto>> GetAll { get; }
    public IQueryHandler<GetSampleNoteByIdQuery, SampleNoteDto?> GetById { get; }

    public SampleNoteQueryUnit(
        IQueryHandler<GetAllSampleNotesQuery, List<SampleNoteDto>> getAll,
        IQueryHandler<GetSampleNoteByIdQuery, SampleNoteDto?> getById)
    {
        GetAll = getAll;
        GetById = getById;
    }
}
