using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Services;

/// <summary>
/// In-memory + CSV-backed store for SampleNote (dev playground only).
/// </summary>
public interface ISampleNoteStore
{
    IReadOnlyList<SampleNote> GetAll();
    SampleNote? GetById(Guid id);
    void Add(SampleNote note);
    bool Update(SampleNote note);
    bool Delete(Guid id);
}
