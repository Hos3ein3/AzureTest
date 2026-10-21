using AzureTest.Application.Messaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Contracts.DTOs.Pages;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Documents.Pages;
using FluentValidation;
using Microsoft.Extensions.Options;
using AzureTest.Infrastructure.Configuration;

namespace AzureTest.Application.Pages;

public record CreatePageCommand(string Title, string Slug, string Content, string? ImagePath) : ICommand<Result<Guid>>;
public class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator(IOptions<DashboardSettings> dashboardSettings, INoSqlUnitOfWork noSql)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Must(s => !dashboardSettings.Value.ReservedSlugs.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("This slug is reserved.")
            .MustAsync(async (s, ct) => {
                var ds = await noSql.UserDashboards.GetAllAsync();
                return !ds.Any(d => d.Slug.Equals(s, StringComparison.OrdinalIgnoreCase));
            }).WithMessage("This slug is already used by a user dashboard.")
            .MustAsync(async (s, ct) => {
                var ps = await noSql.Pages.GetAllAsync();
                return !ps.Any(p => p.Slug.Equals(s, StringComparison.OrdinalIgnoreCase));
            }).WithMessage("This slug is already used by another page.");
    }
}
public class CreatePageCommandHandler : ICommandHandler<CreatePageCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _noSql;
    public CreatePageCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result<Guid>> HandleAsync(CreatePageCommand cmd, CancellationToken ct = default)
    {
        var doc = new PageDocument { Title = cmd.Title, Slug = cmd.Slug, Content = cmd.Content, ImagePath = cmd.ImagePath, Status = PageStatus.Pending };
        await _noSql.Pages.AddAsync(doc);
        return Result.Success(doc.Id);
    }
}

public record UpdatePageCommand(Guid Id, string Title, string Slug, string Content, string? ImagePath, PageStatus Status) : ICommand<Result>;
public class UpdatePageCommandValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageCommandValidator(IOptions<DashboardSettings> dashboardSettings, INoSqlUnitOfWork noSql)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Must(s => !dashboardSettings.Value.ReservedSlugs.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("This slug is reserved.")
            .MustAsync(async (cmd, s, ct) => {
                var ds = await noSql.UserDashboards.GetAllAsync();
                return !ds.Any(d => d.Slug.Equals(s, StringComparison.OrdinalIgnoreCase));
            }).WithMessage("This slug is already used by a user dashboard.")
            .MustAsync(async (cmd, s, ct) => {
                var ps = await noSql.Pages.GetAllAsync();
                return !ps.Any(p => p.Id != cmd.Id && p.Slug.Equals(s, StringComparison.OrdinalIgnoreCase));
            }).WithMessage("This slug is already used by another page.");
    }
}
public class UpdatePageCommandHandler : ICommandHandler<UpdatePageCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public UpdatePageCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(UpdatePageCommand cmd, CancellationToken ct = default)
    {
        var doc = await _noSql.Pages.GetByIdAsync(cmd.Id);
        if(doc == null) return Result.Failure("Not found");
        doc.Title = cmd.Title; doc.Slug = cmd.Slug; doc.Content = cmd.Content; doc.ImagePath = cmd.ImagePath; doc.Status = cmd.Status; doc.UpdatedAt = DateTime.UtcNow;
        await _noSql.Pages.UpdateAsync(doc.Id, doc);
        return Result.Success();
    }
}

public record DeletePageCommand(Guid Id) : ICommand<Result>;
public class DeletePageCommandHandler : ICommandHandler<DeletePageCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public DeletePageCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(DeletePageCommand cmd, CancellationToken ct = default)
    {
        await _noSql.Pages.RemoveAsync(cmd.Id);
        return Result.Success();
    }
}

public record ToggleLikeCommand(Guid PageId, Guid UserId) : ICommand<Result>;
public class ToggleLikeCommandHandler : ICommandHandler<ToggleLikeCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public ToggleLikeCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(ToggleLikeCommand cmd, CancellationToken ct = default)
    {
        var doc = await _noSql.Pages.GetByIdAsync(cmd.PageId);
        if(doc == null) return Result.Failure("Not found");
        if(doc.DislikedBy.Contains(cmd.UserId)) doc.DislikedBy.Remove(cmd.UserId);
        if(doc.LikedBy.Contains(cmd.UserId)) doc.LikedBy.Remove(cmd.UserId);
        else doc.LikedBy.Add(cmd.UserId);
        await _noSql.Pages.UpdateAsync(doc.Id, doc);
        return Result.Success();
    }
}

public record ToggleDislikeCommand(Guid PageId, Guid UserId) : ICommand<Result>;
public class ToggleDislikeCommandHandler : ICommandHandler<ToggleDislikeCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public ToggleDislikeCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(ToggleDislikeCommand cmd, CancellationToken ct = default)
    {
        var doc = await _noSql.Pages.GetByIdAsync(cmd.PageId);
        if(doc == null) return Result.Failure("Not found");
        if(doc.LikedBy.Contains(cmd.UserId)) doc.LikedBy.Remove(cmd.UserId);
        if(doc.DislikedBy.Contains(cmd.UserId)) doc.DislikedBy.Remove(cmd.UserId);
        else doc.DislikedBy.Add(cmd.UserId);
        await _noSql.Pages.UpdateAsync(doc.Id, doc);
        return Result.Success();
    }
}

public record AddCommentCommand(Guid PageId, Guid UserId, string UserName, string Content) : ICommand<Result>;
public class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public AddCommentCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(AddCommentCommand cmd, CancellationToken ct = default)
    {
        var doc = await _noSql.Pages.GetByIdAsync(cmd.PageId);
        if(doc == null) return Result.Failure("Not found");
        doc.Comments.Add(new PageComment { Id = Guid.NewGuid(), UserId = cmd.UserId, UserName = cmd.UserName, Content = cmd.Content, CreatedAt = DateTime.UtcNow });
        await _noSql.Pages.UpdateAsync(doc.Id, doc);
        return Result.Success();
    }
}

public record AnswerCommentCommand(Guid PageId, Guid CommentId, string Reply) : ICommand<Result>;
public class AnswerCommentCommandHandler : ICommandHandler<AnswerCommentCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public AnswerCommentCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;
    public async Task<Result> HandleAsync(AnswerCommentCommand cmd, CancellationToken ct = default)
    {
        var doc = await _noSql.Pages.GetByIdAsync(cmd.PageId);
        if(doc == null) return Result.Failure("Not found");
        var c = doc.Comments.FirstOrDefault(x => x.Id == cmd.CommentId);
        if(c == null) return Result.Failure("Comment not found");
        c.AdminReply = cmd.Reply;
        c.AdminReplyAt = DateTime.UtcNow;
        await _noSql.Pages.UpdateAsync(doc.Id, doc);
        return Result.Success();
    }
}

public record MarkPageAsReadCommand(Guid PageId, Guid UserId) : ICommand<Result>;
public class MarkPageAsReadCommandHandler : ICommandHandler<MarkPageAsReadCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSql;
    public MarkPageAsReadCommandHandler(INoSqlUnitOfWork noSql) => _noSql = noSql;

    public async Task<Result> HandleAsync(MarkPageAsReadCommand cmd, CancellationToken ct = default)
    {
        if (cmd.UserId == Guid.Empty) return Result.Success();
        var doc = await _noSql.Pages.GetByIdAsync(cmd.PageId);
        if (doc == null) return Result.Failure("Not found");
        doc.ReadBy ??= new List<Guid>();
        if (!doc.ReadBy.Contains(cmd.UserId))
        {
            doc.ReadBy.Add(cmd.UserId);
            await _noSql.Pages.UpdateAsync(doc.Id, doc);
        }
        return Result.Success();
    }
}
