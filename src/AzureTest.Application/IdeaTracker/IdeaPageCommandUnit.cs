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

public record CreateIdeaPageCommand(Guid UserId, CreateIdeaPageDto Dto) : ICommand<Result<Guid>>;
public record UpdateIdeaPageCommand(Guid UserId, UpdateIdeaPageDto Dto) : ICommand<Result>;
public record DeleteIdeaPageCommand(Guid UserId, Guid Id) : ICommand<Result>;

public class CreateIdeaPageCommandValidator : AbstractValidator<CreateIdeaPageCommand>
{
    public CreateIdeaPageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}
public class UpdateIdeaPageCommandValidator : AbstractValidator<UpdateIdeaPageCommand>
{
    public UpdateIdeaPageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
    }
}

public class CreateIdeaPageCommandHandler : ICommandHandler<CreateIdeaPageCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public CreateIdeaPageCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> HandleAsync(CreateIdeaPageCommand command, CancellationToken ct = default)
    {
        // verify idea
        var idea = await _unitOfWork.Ideas.GetByIdAsync(command.Dto.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result<Guid>.Failure("Idea not found or unauthorized.");

        var page = new IdeaPage
        {
            IdeaId = command.Dto.IdeaId,
            Title = command.Dto.Title,
            Content = command.Dto.Content,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.IdeaPages.AddAsync(page, ct);
        return Result<Guid>.Success(page.Id);
    }
}

public class UpdateIdeaPageCommandHandler : ICommandHandler<UpdateIdeaPageCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaPageCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaPageCommand command, CancellationToken ct = default)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(command.Dto.Id, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        page.Title = command.Dto.Title;
        page.Content = command.Dto.Content;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.IdeaPages.UpdateAsync(page.Id, page, ct);
        return Result.Success();
    }
}

public class DeleteIdeaPageCommandHandler : ICommandHandler<DeleteIdeaPageCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public DeleteIdeaPageCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(DeleteIdeaPageCommand command, CancellationToken ct = default)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(command.Id, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != command.UserId) return Result.Failure("Idea not found or unauthorized.");

        page.IsDeleted = true;
        page.DeletedOn = DateTime.UtcNow;
        await _unitOfWork.IdeaPages.UpdateAsync(page.Id, page, ct);
        return Result.Success();
    }
}

public interface IIdeaPageCommandUnit
{
    ICommandHandler<CreateIdeaPageCommand, Result<Guid>> Create { get; }
    ICommandHandler<UpdateIdeaPageCommand, Result> Update { get; }
    ICommandHandler<DeleteIdeaPageCommand, Result> Delete { get; }
}

public class IdeaPageCommandUnit : IIdeaPageCommandUnit
{
    public ICommandHandler<CreateIdeaPageCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateIdeaPageCommand, Result> Update { get; }
    public ICommandHandler<DeleteIdeaPageCommand, Result> Delete { get; }

    public IdeaPageCommandUnit(
        ICommandHandler<CreateIdeaPageCommand, Result<Guid>> create,
        ICommandHandler<UpdateIdeaPageCommand, Result> update,
        ICommandHandler<DeleteIdeaPageCommand, Result> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}


public record UpdateIdeaPageLayoutDto(Guid Id, int X, int Y, int Width, int Height);
public record UpdateIdeaPageLayoutCommand(Guid UserId, UpdateIdeaPageLayoutDto Dto) : ICommand<Result>;

public class UpdateIdeaPageLayoutCommandHandler : ICommandHandler<UpdateIdeaPageLayoutCommand, Result>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public UpdateIdeaPageLayoutCommandHandler(INoSqlUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(UpdateIdeaPageLayoutCommand command, CancellationToken ct = default)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(command.Dto.Id, ct);
        if (page == null || page.IsDeleted) return Result.Failure("Page not found.");
        
        page.X = command.Dto.X;
        page.Y = command.Dto.Y;
        page.Width = command.Dto.Width;
        page.Height = command.Dto.Height;
        
        await _unitOfWork.IdeaPages.UpdateAsync(page.Id, page, ct);
        return Result.Success();
    }
}

