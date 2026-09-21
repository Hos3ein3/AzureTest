using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Application.Resumes.Commands.UpdateResumeStatus;

namespace AzureTest.Application.Resumes;

public sealed class ResumeCommandUnit : IResumeCommandUnit
{
    public ICommandHandler<CreateResumeCommand, Guid> Create { get; }
    public ICommandHandler<UpdateResumeCommand> Update { get; }
    public ICommandHandler<UpdateResumeStatusCommand> UpdateStatus { get; }
    public ICommandHandler<DeleteResumeCommand> Delete { get; }

    public ResumeCommandUnit(
        ICommandHandler<CreateResumeCommand, Guid> create,
        ICommandHandler<UpdateResumeCommand> update,
        ICommandHandler<UpdateResumeStatusCommand> updateStatus,
        ICommandHandler<DeleteResumeCommand> delete)
    {
        Create = create;
        Update = update;
        UpdateStatus = updateStatus;
        Delete = delete;
    }
}
