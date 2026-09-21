using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.CreateResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateResumeCommand(CreateResumeDto Dto) : ICommand<Guid>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateResumeCommandHandler : BaseCommandHandler<CreateResumeCommand, Guid>
{
    private readonly IResumeRepository _resumeRepository;

    public CreateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ILogger<CreateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task<Guid> ExecuteAsync(CreateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        var resume = Resume.Create(
            dto.PositionTitle,
            dto.Description,
            dto.Location,
            dto.JobLink,
            dto.Note,
            dto.Source,
            dto.Status);

        resume.CreatedOn = dto.CreatedOn;

        _resumeRepository.Add(resume);

        return await Task.FromResult(resume.Id);
    }
}
