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

public record GetIdeaPagesQuery(Guid UserId, Guid IdeaId) : IQuery<Result<List<IdeaPageDto>>>;
public record GetIdeaPageByIdQuery(Guid UserId, Guid Id) : IQuery<Result<IdeaPageDto>>;

public class GetIdeaPagesQueryHandler : BaseQueryHandler<GetIdeaPagesQuery, Result<List<IdeaPageDto>>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeaPagesQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeaPagesQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<List<IdeaPageDto>>> ExecuteAsync(GetIdeaPagesQuery query, CancellationToken ct)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(query.IdeaId, ct);
        if (idea == null || idea.UserId != query.UserId) return Result<List<IdeaPageDto>>.Failure("Idea not found or unauthorized.");

        var records = await _unitOfWork.IdeaPages.FindAsync(x => x.IdeaId == query.IdeaId && !x.IsDeleted);
        var result = records.OrderByDescending(x => x.CreatedAt).Select(MapToDto).ToList();
        return Result<List<IdeaPageDto>>.Success(result);
    }
    
    public static IdeaPageDto MapToDto(IdeaPage doc) => new(
        doc.Id, doc.IdeaId, doc.Title, doc.Content, doc.CreatedAt, doc.X, doc.Y, doc.Width, doc.Height, doc.UpdatedAt
    );
}

public class GetIdeaPageByIdQueryHandler : BaseQueryHandler<GetIdeaPageByIdQuery, Result<IdeaPageDto>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeaPageByIdQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeaPageByIdQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<IdeaPageDto>> ExecuteAsync(GetIdeaPageByIdQuery query, CancellationToken ct)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(query.Id, ct);
        if (page == null || page.IsDeleted) return Result<IdeaPageDto>.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != query.UserId) return Result<IdeaPageDto>.Failure("Idea not found or unauthorized.");
        
        return Result<IdeaPageDto>.Success(GetIdeaPagesQueryHandler.MapToDto(page));
    }
}

public interface IIdeaPageQueryUnit
{
    IQueryHandler<GetIdeaPagesQuery, Result<List<IdeaPageDto>>> GetIdeaPages { get; }
    IQueryHandler<GetIdeaPageByIdQuery, Result<IdeaPageDto>> GetIdeaPageById { get; }
}

public class IdeaPageQueryUnit : IIdeaPageQueryUnit
{
    public IQueryHandler<GetIdeaPagesQuery, Result<List<IdeaPageDto>>> GetIdeaPages { get; }
    public IQueryHandler<GetIdeaPageByIdQuery, Result<IdeaPageDto>> GetIdeaPageById { get; }

    public IdeaPageQueryUnit(
        IQueryHandler<GetIdeaPagesQuery, Result<List<IdeaPageDto>>> getIdeaPages,
        IQueryHandler<GetIdeaPageByIdQuery, Result<IdeaPageDto>> getIdeaPageById)
    {
        GetIdeaPages = getIdeaPages;
        GetIdeaPageById = getIdeaPageById;
    }
}
