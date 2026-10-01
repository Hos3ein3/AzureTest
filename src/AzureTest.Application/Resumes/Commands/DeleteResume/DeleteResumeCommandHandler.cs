using AzureTest.Application.Abstractions;


using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.DeleteResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeleteResumeCommand(Guid Id) : ICommand<Result<bool>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeleteResumeCommandHandler : BaseCommandHandler<DeleteResumeCommand, Result<bool>>
{

    private readonly ICurrentUser _currentUser;

    public DeleteResumeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<DeleteResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<bool>> ExecuteAsync(DeleteResumeCommand command, CancellationToken cancellationToken)
    {
        var resume = await UnitOfWork.Resumes.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Id} was not found.");

        if (resume.UserId != _currentUser.Id)
            throw new UnauthorizedAccessException("You are not authorized to delete this resume.");

        resume.SoftDelete(_currentUser.Id.ToString());
        UnitOfWork.Resumes.Update(resume);
        return  true;
    }
}
