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

public record GetIdeaSectionsQuery(Guid UserId, Guid IdeaPageId) : IQuery<Result<List<IdeaSectionDto>>>;
public record GetIdeaSectionByIdQuery(Guid UserId, Guid Id) : IQuery<Result<IdeaSectionDto>>;

public class GetIdeaSectionsQueryHandler : BaseQueryHandler<GetIdeaSectionsQuery, Result<List<IdeaSectionDto>>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeaSectionsQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeaSectionsQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<List<IdeaSectionDto>>> ExecuteAsync(GetIdeaSectionsQuery query, CancellationToken ct)
    {
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(query.IdeaPageId, ct);
        if (page == null || page.IsDeleted) return Result<List<IdeaSectionDto>>.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != query.UserId) return Result<List<IdeaSectionDto>>.Failure("Idea not found or unauthorized.");

        var records = await _unitOfWork.IdeaSections.FindAsync(x => x.IdeaPageId == query.IdeaPageId && !x.IsDeleted);
        var result = records.OrderBy(x => x.OrderIndex).Select(MapToDto).ToList();
        return Result<List<IdeaSectionDto>>.Success(result);
    }
    
    public static IdeaSectionDto MapToDto(IdeaSection doc) => new(
        doc.Id, doc.IdeaPageId, doc.Title, doc.Content, doc.OrderIndex, doc.CreatedAt, doc.X, doc.Y, doc.Width, doc.Height, doc.UpdatedAt
    );
}

public class GetIdeaSectionByIdQueryHandler : BaseQueryHandler<GetIdeaSectionByIdQuery, Result<IdeaSectionDto>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    public GetIdeaSectionByIdQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetIdeaSectionByIdQueryHandler> logger) : base(logger)
        => _unitOfWork = unitOfWork;

    protected override async Task<Result<IdeaSectionDto>> ExecuteAsync(GetIdeaSectionByIdQuery query, CancellationToken ct)
    {
        var section = await _unitOfWork.IdeaSections.GetByIdAsync(query.Id, ct);
        if (section == null || section.IsDeleted) return Result<IdeaSectionDto>.Failure("Section not found.");
        
        var page = await _unitOfWork.IdeaPages.GetByIdAsync(section.IdeaPageId, ct);
        if (page == null || page.IsDeleted) return Result<IdeaSectionDto>.Failure("Page not found.");
        
        var idea = await _unitOfWork.Ideas.GetByIdAsync(page.IdeaId, ct);
        if (idea == null || idea.UserId != query.UserId) return Result<IdeaSectionDto>.Failure("Idea not found or unauthorized.");
        
        return Result<IdeaSectionDto>.Success(GetIdeaSectionsQueryHandler.MapToDto(section));
    }
}

public interface IIdeaSectionQueryUnit
{
    IQueryHandler<GetIdeaSectionsQuery, Result<List<IdeaSectionDto>>> GetIdeaSections { get; }
    IQueryHandler<GetIdeaSectionByIdQuery, Result<IdeaSectionDto>> GetIdeaSectionById { get; }
}

public class IdeaSectionQueryUnit : IIdeaSectionQueryUnit
{
    public IQueryHandler<GetIdeaSectionsQuery, Result<List<IdeaSectionDto>>> GetIdeaSections { get; }
    public IQueryHandler<GetIdeaSectionByIdQuery, Result<IdeaSectionDto>> GetIdeaSectionById { get; }

    public IdeaSectionQueryUnit(
        IQueryHandler<GetIdeaSectionsQuery, Result<List<IdeaSectionDto>>> getIdeaSections,
        IQueryHandler<GetIdeaSectionByIdQuery, Result<IdeaSectionDto>> getIdeaSectionById)
    {
        GetIdeaSections = getIdeaSections;
        GetIdeaSectionById = getIdeaSectionById;
    }
}
