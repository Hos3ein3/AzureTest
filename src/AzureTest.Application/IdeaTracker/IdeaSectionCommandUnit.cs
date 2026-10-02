using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.IdeaTracker;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.IdeaTracker;

public record CreateIdeaSectionCommand(Guid UserId, CreateIdeaSectionDto Dto) : ICommand<Result<Guid>>;
public record UpdateIdeaSectionCommand(Guid UserId, UpdateIdeaSectionDto Dto) : ICommand<Result>;
public record DeleteIdeaSectionCommand(Guid UserId, Guid Id) : ICommand<Result>;

public class CreateIdeaSectionCommandValidator : AbstractValidator<CreateIdeaSectionCommand>
{
    public CreateIdeaSectionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}
public class UpdateIdeaSectionCommandValidator : AbstractValidator<UpdateIdeaSectionCommand>
{
    public UpdateIdeaSectionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}

public class CreateIdeaSectionCommandHandler : ICommandHandler<CreateIdeaSectionCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public CreateIdeaSectionCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> HandleAsync(CreateIdeaSectionCommand command, CancellationToken ct = default)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(command.Dto.IdeaPageId, ct);
        if (page == null || page.IsDeleted) return Result<Guid>.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result<Guid>.Failure("Idea not found or unauthorized.");

        var section = new IdeaSection
        {
            IdeaPageId = command.Dto.IdeaPageId,
            Title = command.Dto.Title,
            Content = command.Dto.Content,
            OrderIndex = command.Dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.IdeaSections.AddAsync(section, ct);
        return Result<Guid>.Success(section.Id);
    }
}

public class UpdateIdeaSectionCommandHandler : ICommandHandler<UpdateIdeaSectionCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaSectionCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaSectionCommand command, CancellationToken ct = default)
    {
        var section = await _unitOfWork.IdeaSections.GetByIdAsync(command.Dto.Id, ct);
        if (section == null || section.IsDeleted) return Result.Failure("Section not found.");
        
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(section.IdeaPageId, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        section.Title = command.Dto.Title;
        section.Content = command.Dto.Content;
        section.OrderIndex = command.Dto.OrderIndex;
        section.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.IdeaSections.UpdateAsync(section.Id, section, ct);
        return Result.Success();
    }
}

public class DeleteIdeaSectionCommandHandler : ICommandHandler<DeleteIdeaSectionCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public DeleteIdeaSectionCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(DeleteIdeaSectionCommand command, CancellationToken ct = default)
    {
        var section = await _unitOfWork.IdeaSections.GetByIdAsync(command.Id, ct);
        if (section == null || section.IsDeleted) return Result.Failure("Section not found.");
        
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(section.IdeaPageId, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        section.IsDeleted = true;
        section.DeletedOn = DateTime.UtcNow;
        await _unitOfWork.IdeaSections.UpdateAsync(section.Id, section, ct);
        return Result.Success();
    }
}

public interface IIdeaSectionCommandUnit
{
    ICommandHandler<CreateIdeaSectionCommand, Result<Guid>> Create { get; }
    ICommandHandler<UpdateIdeaSectionCommand, Result> Update { get; }
    ICommandHandler<DeleteIdeaSectionCommand, Result> Delete { get; }
}

public class IdeaSectionCommandUnit : IIdeaSectionCommandUnit
{
    public ICommandHandler<CreateIdeaSectionCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateIdeaSectionCommand, Result> Update { get; }
    public ICommandHandler<DeleteIdeaSectionCommand, Result> Delete { get; }

    public IdeaSectionCommandUnit(
        ICommandHandler<CreateIdeaSectionCommand, Result<Guid>> create,
        ICommandHandler<UpdateIdeaSectionCommand, Result> update,
        ICommandHandler<DeleteIdeaSectionCommand, Result> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}


public record UpdateIdeaSectionLayoutDto(Guid Id, int X, int Y, int Width, int Height);
public record UpdateIdeaSectionLayoutCommand(Guid UserId, UpdateIdeaSectionLayoutDto Dto) : ICommand<Result>;

public class UpdateIdeaSectionLayoutCommandHandler : ICommandHandler<UpdateIdeaSectionLayoutCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaSectionLayoutCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaSectionLayoutCommand command, CancellationToken ct = default)
    {
        var sec = await _unitOfWork.IdeaSections.GetByIdAsync(command.Dto.Id, ct);
        if (sec == null || sec.IsDeleted) return Result.Failure("Section not found.");
        
        sec.X = command.Dto.X;
        sec.Y = command.Dto.Y;
        sec.Width = command.Dto.Width;
        sec.Height = command.Dto.Height;
        
        await _unitOfWork.IdeaSections.UpdateAsync(sec.Id, sec, ct);
        return Result.Success();
    }
}

