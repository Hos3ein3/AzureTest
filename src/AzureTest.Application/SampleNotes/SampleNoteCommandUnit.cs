using AzureTest.Application.Messaging;
using AzureTest.Application.SampleNotes.Commands;

namespace AzureTest.Application.SampleNotes;

public interface ISampleNoteCommandUnit
{
    ICommandHandler<CreateSampleNoteCommand, Guid> Create { get; }
    ICommandHandler<UpdateSampleNoteCommand, bool> Update { get; }
    ICommandHandler<DeleteSampleNoteCommand, bool> Delete { get; }
}

public sealed class SampleNoteCommandUnit : ISampleNoteCommandUnit
{
    public ICommandHandler<CreateSampleNoteCommand, Guid> Create { get; }
    public ICommandHandler<UpdateSampleNoteCommand, bool> Update { get; }
    public ICommandHandler<DeleteSampleNoteCommand, bool> Delete { get; }

    public SampleNoteCommandUnit(
        ICommandHandler<CreateSampleNoteCommand, Guid> create,
        ICommandHandler<UpdateSampleNoteCommand, bool> update,
        ICommandHandler<DeleteSampleNoteCommand, bool> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
