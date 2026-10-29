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

public class PageFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public string? Search
    {
        get => SearchTerm;
        set => SearchTerm = value;
    }
    public PageStatus? Status { get; set; }

    public PageFilter()
    {
        SortBy = "Title";
        SortDescending = false;
    }
}

public record GetAllPagesQuery(PageFilter Filter) : IQuery<Result<AzureTest.Contracts.DTOs.Common.PagedResult<PageDto, PageFilter>>>;
public class GetAllPagesQueryHandler : IQueryHandler<GetAllPagesQuery, Result<AzureTest.Contracts.DTOs.Common.PagedResult<PageDto, PageFilter>>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    public GetAllPagesQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork) => _noSqlUnitOfWork = noSqlUnitOfWork;

    public async Task<Result<AzureTest.Contracts.DTOs.Common.PagedResult<PageDto, PageFilter>>> HandleAsync(GetAllPagesQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<PageDocument> all = await _noSqlUnitOfWork.Pages.GetAllAsync();
        
        if (query.Filter.Status.HasValue)
            all = all.Where(x => x.Status == query.Filter.Status.Value);
            
        if (!string.IsNullOrWhiteSpace(query.Filter.Search))
            all = all.Where(x => x.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase) || x.Slug.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase));

        var dtos = all.Select(Map).AsQueryable();

        string sortBy = (query.Filter.SortBy ?? "Title").ToLower();
        bool desc = query.Filter.SortDescending;

        // If sorting by newest/createdat/date without explicit SortDescending specified, default to descending (newest first)
        if ((sortBy is "createdat" or "date" or "newest") && !query.Filter.SortDescending && query.Filter.SortBy?.ToLower() is "createdat" or "date" or "newest")
        {
            desc = true;
        }

        dtos = sortBy switch
        {
            "slug" => desc ? dtos.OrderByDescending(x => x.Slug) : dtos.OrderBy(x => x.Slug),
            "status" => desc ? dtos.OrderByDescending(x => x.Status) : dtos.OrderBy(x => x.Status),
            "createdat" or "date" or "newest" => desc ? dtos.OrderByDescending(x => x.CreatedAt) : dtos.OrderBy(x => x.CreatedAt),
            _ => desc ? dtos.OrderByDescending(x => x.Title) : dtos.OrderBy(x => x.Title)
        };

        var dtoList = dtos.ToList();
        int total = dtoList.Count;
        int currentPage = query.Filter.PageNumber < 1 ? 1 : query.Filter.PageNumber;
        int pageSize = query.Filter.PageSize < 1 ? 10 : query.Filter.PageSize;

        var items = (query.Filter.PageSize <= 0)
            ? dtoList
            : dtoList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        var pagedResult = new AzureTest.Contracts.DTOs.Common.PagedResult<PageDto, PageFilter>(items, total, currentPage, pageSize, query.Filter);
        return Result.Success(pagedResult);
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
        ReadBy = doc.ReadBy ?? new(),
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
        return Result.Success(new PageDto { Id = doc.Id, Title = doc.Title, Slug = doc.Slug, Content = doc.Content, ImagePath = doc.ImagePath, Status = doc.Status, LikedBy = doc.LikedBy, DislikedBy = doc.DislikedBy, ReadBy = doc.ReadBy ?? new(), CreatedAt = doc.CreatedAt, UpdatedAt = doc.UpdatedAt, Comments = doc.Comments.Select(c => new PageCommentDto { Id = c.Id, UserId = c.UserId, UserName = c.UserName, Content = c.Content, CreatedAt = c.CreatedAt, AdminReply = c.AdminReply, AdminReplyAt = c.AdminReplyAt }).ToList() });
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
            ReadBy = doc.ReadBy ?? new(),
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
