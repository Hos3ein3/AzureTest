using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.SampleNotes.Commands;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.SampleNotes;

public interface ISampleNoteCommandUnit
{
    ICommandHandler<CreateSampleNoteCommand, Result<SampleNote>> Create { get; }
    ICommandHandler<UpdateSampleNoteCommand, Result> Update { get; }
    ICommandHandler<DeleteSampleNoteCommand, Result> Delete { get; }
}

public sealed class SampleNoteCommandUnit : ISampleNoteCommandUnit
{
    public ICommandHandler<CreateSampleNoteCommand, Result<SampleNote>> Create { get; }
    public ICommandHandler<UpdateSampleNoteCommand, Result> Update { get; }
    public ICommandHandler<DeleteSampleNoteCommand, Result> Delete { get; }

    public SampleNoteCommandUnit(
        ICommandHandler<CreateSampleNoteCommand, Result<SampleNote>> create,
        ICommandHandler<UpdateSampleNoteCommand, Result> update,
        ICommandHandler<DeleteSampleNoteCommand, Result> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
