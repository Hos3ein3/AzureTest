using AzureTest.Application.Abstractions;


using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Application.Resumes.Commands.UpdateResumeStatus;

namespace AzureTest.Application.Resumes;

public sealed class ResumeCommandUnit : IResumeCommandUnit
{
    public ICommandHandler<CreateResumeCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateResumeCommand, Result<bool>> Update { get; }
    public ICommandHandler<UpdateResumeStatusCommand> UpdateStatus { get; }
    public ICommandHandler<DeleteResumeCommand, Result<bool>> Delete { get; }

    public ResumeCommandUnit(
        ICommandHandler<CreateResumeCommand, Result<Guid>> create,
        ICommandHandler<UpdateResumeCommand, Result<bool>> update,
        ICommandHandler<UpdateResumeStatusCommand> updateStatus,
        ICommandHandler<DeleteResumeCommand, Result<bool>> delete)
    {
        Create = create;
        Update = update;
        UpdateStatus = updateStatus;
        Delete = delete;
    }
}
