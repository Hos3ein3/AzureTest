using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Application.Resumes.Commands.UpdateResumeStatus;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Resumes;

/// <summary>
/// Aggregates all Resume command handlers into a single injectable unit.
/// Inspired by the Unit of Work pattern — avoids polluting controllers with individual handler injections.
/// </summary>
public interface IResumeCommandUnit
{
    ICommandHandler<CreateResumeCommand, Guid> Create { get; }
    ICommandHandler<UpdateResumeCommand,bool> Update { get; }
    ICommandHandler<UpdateResumeStatusCommand> UpdateStatus { get; }
    ICommandHandler<DeleteResumeCommand,bool> Delete { get; }
}
