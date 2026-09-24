using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.DeleteResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeleteResumeCommand(Guid Id) : ICommand<bool>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeleteResumeCommandHandler : BaseCommandHandler<DeleteResumeCommand,bool>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ICurrentUser currentUser,
        ILogger<DeleteResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(DeleteResumeCommand command, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Id} was not found.");

        if (resume.UserId != _currentUser.Id)
            throw new UnauthorizedAccessException("You are not authorized to delete this resume.");

        resume.SoftDelete(_currentUser.Id.ToString());
        _resumeRepository.Update(resume);
        return  true;
    }
}
