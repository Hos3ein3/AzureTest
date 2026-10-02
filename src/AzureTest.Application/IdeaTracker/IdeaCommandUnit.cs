using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.IdeaTracker;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;

using FluentValidation;

namespace AzureTest.Application.IdeaTracker;

// --- COMMANDS ---
public record CreateIdeaCommand(Guid UserId, CreateIdeaDto Dto) : ICommand<Result<Guid>>;
public record UpdateIdeaCommand(Guid UserId, UpdateIdeaDto Dto) : ICommand<Result>;
public record DeleteIdeaCommand(Guid UserId, Guid Id) : ICommand<Result>;

// --- VALIDATORS ---
public class CreateIdeaCommandValidator : AbstractValidator<CreateIdeaCommand>
{
    public CreateIdeaCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}
public class UpdateIdeaCommandValidator : AbstractValidator<UpdateIdeaCommand>
{
    public UpdateIdeaCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}

// --- HANDLERS ---
public class CreateIdeaCommandHandler : ICommandHandler<CreateIdeaCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public CreateIdeaCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> HandleAsync(CreateIdeaCommand command, CancellationToken ct = default)
    {
        var idea = new Idea
        {
            UserId = command.UserId,
            ParentId = command.Dto.ParentId,
            Title = command.Dto.Title,
            Content = command.Dto.Content,
            Icon = command.Dto.Icon,
            CoverImage = command.Dto.CoverImage,
            ThemeColor = command.Dto.ThemeColor,
            Status = (IdeaStatus)command.Dto.Status,
            Priority = (IdeaPriority)command.Dto.Priority,
            Tags = command.Dto.Tags ?? new(),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Ideas.AddAsync(idea, ct);
        return Result<Guid>.Success(idea.Id);
    }
}

public class UpdateIdeaCommandHandler : ICommandHandler<UpdateIdeaCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaCommand command, CancellationToken ct = default)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(command.Dto.Id, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        idea.ParentId = command.Dto.ParentId;
        idea.Title = command.Dto.Title;
        idea.Content = command.Dto.Content;
        idea.Icon = command.Dto.Icon;
        idea.CoverImage = command.Dto.CoverImage;
        idea.ThemeColor = command.Dto.ThemeColor;
        idea.Status = (IdeaStatus)command.Dto.Status;
        idea.Priority = (IdeaPriority)command.Dto.Priority;
        idea.Tags = command.Dto.Tags ?? new();
        idea.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Ideas.UpdateAsync(idea.Id, idea, ct);
        return Result.Success();
    }
}

public class DeleteIdeaCommandHandler : ICommandHandler<DeleteIdeaCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public DeleteIdeaCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(DeleteIdeaCommand command, CancellationToken ct = default)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(command.Id, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        await _unitOfWork.Ideas.RemoveAsync(command.Id, ct);
        return Result.Success();
    }
}

// --- UNIT ---
public interface IIdeaCommandUnit
{
    ICommandHandler<CreateIdeaCommand, Result<Guid>> Create { get; }
    ICommandHandler<UpdateIdeaCommand, Result> Update { get; }
    ICommandHandler<DeleteIdeaCommand, Result> Delete { get; }
}
public class IdeaCommandUnit : IIdeaCommandUnit
{
    public ICommandHandler<CreateIdeaCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateIdeaCommand, Result> Update { get; }
    public ICommandHandler<DeleteIdeaCommand, Result> Delete { get; }

    public IdeaCommandUnit(
        ICommandHandler<CreateIdeaCommand, Result<Guid>> create,
        ICommandHandler<UpdateIdeaCommand, Result> update,
        ICommandHandler<DeleteIdeaCommand, Result> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
