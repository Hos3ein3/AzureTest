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

namespace AzureTest.Application.Pages;

public class PageFilter
{
    public string? Search { get; set; }
    public PageStatus? Status { get; set; }
}

public record GetAllPagesQuery(PageFilter Filter) : IQuery<Result<List<PageDto>>>;
public class GetAllPagesQueryHandler : IQueryHandler<GetAllPagesQuery, Result<List<PageDto>>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    public GetAllPagesQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork) => _noSqlUnitOfWork = noSqlUnitOfWork;

    public async Task<Result<List<PageDto>>> HandleAsync(GetAllPagesQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<PageDocument> all = await _noSqlUnitOfWork.Pages.GetAllAsync();
        
        if (query.Filter.Status.HasValue)
            all = all.Where(x => x.Status == query.Filter.Status.Value);
            
        if (!string.IsNullOrWhiteSpace(query.Filter.Search))
            all = all.Where(x => x.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase) || x.Slug.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase));

        return Result.Success(all.Select(Map).ToList());
    }
    
    private static PageDto Map(PageDocument doc) => new()
    {
        Id = doc.Id,
        Title = doc.Title,
        Slug = doc.Slug,
        Content = doc.Content,
        ImagePath = doc.ImagePath,
        Status = doc.Status,
        LikedBy = doc.LikedBy,
        DislikedBy = doc.DislikedBy,
        CreatedAt = doc.CreatedAt,
        UpdatedAt = doc.UpdatedAt,
        Comments = doc.Comments.Select(c => new PageCommentDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.UserName,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            AdminReply = c.AdminReply,
            AdminReplyAt = c.AdminReplyAt
        }).ToList()
    };
}

public record GetPageByIdQuery(Guid Id) : IQuery<Result<PageDto>>;
public class GetPageByIdQueryHandler : IQueryHandler<GetPageByIdQuery, Result<PageDto>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    public GetPageByIdQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork) => _noSqlUnitOfWork = noSqlUnitOfWork;

    public async Task<Result<PageDto>> HandleAsync(GetPageByIdQuery query, CancellationToken cancellationToken = default)
    {
        var doc = await _noSqlUnitOfWork.Pages.GetByIdAsync(query.Id);
        if (doc == null) return Result.Failure<PageDto>("Page not found.");
        return Result.Success(new PageDto { Id = doc.Id, Title = doc.Title, Slug = doc.Slug, Content = doc.Content, ImagePath = doc.ImagePath, Status = doc.Status, LikedBy = doc.LikedBy, DislikedBy = doc.DislikedBy, CreatedAt = doc.CreatedAt, UpdatedAt = doc.UpdatedAt, Comments = doc.Comments.Select(c => new PageCommentDto { Id = c.Id, UserId = c.UserId, UserName = c.UserName, Content = c.Content, CreatedAt = c.CreatedAt, AdminReply = c.AdminReply, AdminReplyAt = c.AdminReplyAt }).ToList() }); // reuse mapper but wait, it's private. Let's redefine.
    }
}

public record GetPageBySlugQuery(string Slug) : IQuery<Result<PageDto>>;
public class GetPageBySlugQueryHandler : IQueryHandler<GetPageBySlugQuery, Result<PageDto>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    public GetPageBySlugQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork) => _noSqlUnitOfWork = noSqlUnitOfWork;

    public async Task<Result<PageDto>> HandleAsync(GetPageBySlugQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<PageDocument> all = await _noSqlUnitOfWork.Pages.GetAllAsync();
        var doc = all.FirstOrDefault(x => x.Slug.Equals(query.Slug, StringComparison.OrdinalIgnoreCase));
        if (doc == null) return Result.Failure<PageDto>("Page not found.");
        
        return Result.Success(new PageDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Slug = doc.Slug,
            Content = doc.Content,
            ImagePath = doc.ImagePath,
            Status = doc.Status,
            LikedBy = doc.LikedBy,
            DislikedBy = doc.DislikedBy,
            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,
            Comments = doc.Comments.Select(c => new PageCommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.UserName,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AdminReply = c.AdminReply,
                AdminReplyAt = c.AdminReplyAt
            }).ToList()
        });
    }
}
