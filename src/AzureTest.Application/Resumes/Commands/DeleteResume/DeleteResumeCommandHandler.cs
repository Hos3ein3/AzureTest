using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.DeleteResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record DeleteResumeCommand(Guid Id) : ICommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class DeleteResumeCommandHandler : BaseCommandHandler<DeleteResumeCommand>
{
    private readonly IResumeRepository _resumeRepository;

    public DeleteResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ILogger<DeleteResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task ExecuteAsync(DeleteResumeCommand command, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Id} was not found.");

        resume.SoftDelete();
        _resumeRepository.Update(resume);
    }
}
