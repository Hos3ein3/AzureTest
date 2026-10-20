using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.IdeaTracker;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.IdeaTracker;

// --- QUERIES & FILTERS ---
public class IdeaFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public Guid UserId { get; set; }
    public string? Search { get; set; }
    public int? Status { get; set; }

    public IdeaFilter() { }
    public IdeaFilter(Guid userId, string? search = null, int? status = null)
    {
        UserId = userId;
        Search = search;
        Status = status;
        SearchTerm = search;
    }
}
public record GetIdeasQuery(IdeaFilter Filter) : IQuery<Result<List<IdeaDto>>>;
public record GetIdeaByIdQuery(Guid UserId, Guid Id) : IQuery<Result<IdeaDto>>;

// --- HANDLERS ---
public class GetIdeasQueryHandler : BaseQueryHandler<GetIdeasQuery, Result<List<IdeaDto>>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeasQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeasQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<List<IdeaDto>>> ExecuteAsync(GetIdeasQuery query, CancellationToken ct)
    {
        var records = await _unitOfWork.Ideas.FindAsync(x => x.UserId == query.Filter.UserId);
        
        if (!string.IsNullOrWhiteSpace(query.Filter.Search))
        {
            records = records.Where(x => (x.Title != null && x.Title != null && x.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase)) || (x.Content != null && x.Content != null && x.Content.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase))).ToList();
        }
        if (query.Filter.Status.HasValue)
        {
            records = records.Where(x => (int)x.Status == query.Filter.Status.Value).ToList();
        }

        var result = records.OrderByDescending(x => x.CreatedAt).Select(MapToDto).ToList();
        return Result<List<IdeaDto>>.Success(result);
    }
    
    public static IdeaDto MapToDto(Idea doc) => new(
        doc.Id, doc.UserId, doc.ParentId, doc.Title, doc.Content, doc.Icon, doc.CoverImage, doc.ThemeColor,
        (int)doc.Status, (int)doc.Priority, doc.Tags, doc.Tasks.Select(t => new IdeaTaskDto(t.Id, t.Description, t.IsCompleted)).ToList(),
        doc.CreatedAt, doc.UpdatedAt
    );
}

public class GetIdeaByIdQueryHandler : BaseQueryHandler<GetIdeaByIdQuery, Result<IdeaDto>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeaByIdQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeaByIdQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<IdeaDto>> ExecuteAsync(GetIdeaByIdQuery query, CancellationToken ct)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(query.Id, ct);
        if (idea == null || idea.UserId != query.UserId) return Result<IdeaDto>.Failure("Idea not found or unauthorized.");
        return Result<IdeaDto>.Success(GetIdeasQueryHandler.MapToDto(idea));
    }
}

// --- UNIT ---
public interface IIdeaQueryUnit
{
    IQueryHandler<GetIdeasQuery, Result<List<IdeaDto>>> GetIdeas { get; }
    IQueryHandler<GetIdeaByIdQuery, Result<IdeaDto>> GetIdeaById { get; }
}
public class IdeaQueryUnit : IIdeaQueryUnit
{
    public IQueryHandler<GetIdeasQuery, Result<List<IdeaDto>>> GetIdeas { get; }
    public IQueryHandler<GetIdeaByIdQuery, Result<IdeaDto>> GetIdeaById { get; }

    public IdeaQueryUnit(
        IQueryHandler<GetIdeasQuery, Result<List<IdeaDto>>> getIdeas,
        IQueryHandler<GetIdeaByIdQuery, Result<IdeaDto>> getIdeaById)
    {
        GetIdeas = getIdeas;
        GetIdeaById = getIdeaById;
    }
}
