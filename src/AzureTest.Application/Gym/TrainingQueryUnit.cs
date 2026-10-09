using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public class TrainingFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public TrainingFilter()
    {
        SortBy = "Name";
        SortDescending = false;
        PageSize = 10;
    }
}

public record GetAllTrainingsQuery(TrainingFilter? Filter = null) : IQuery<Result<AzureTest.Contracts.DTOs.Common.PagedResult<Training, TrainingFilter>>>;
public class GetAllTrainingsQueryHandler : BaseQueryHandler<GetAllTrainingsQuery, Result<AzureTest.Contracts.DTOs.Common.PagedResult<Training, TrainingFilter>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetAllTrainingsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllTrainingsQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<AzureTest.Contracts.DTOs.Common.PagedResult<Training, TrainingFilter>>> ExecuteAsync(GetAllTrainingsQuery request, CancellationToken ct) {
        var filter = request.Filter ?? new TrainingFilter();
        var items = await _uow.Trainings.GetAllAsync();
        var query = items.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm;
            query = query.Where(x => (x.Name != null && x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) || 
                                     (x.Description != null && x.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        string sortBy = filter.SortBy ?? "Name";
        bool desc = filter.SortDescending;
        query = (sortBy, desc) switch
        {
            ("Type", true) => query.OrderByDescending(x => x.Type.ToString()),
            ("Type", false) => query.OrderBy(x => x.Type.ToString()),
            (_, true) => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };

        var list = query.ToList();
        int totalItems = list.Count;
        int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
        int currentPage = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var pagedList = (filter.PageSize <= 0)
            ? list
            : list.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        return Result.Success(new AzureTest.Contracts.DTOs.Common.PagedResult<Training, TrainingFilter>(pagedList, totalItems, currentPage, pageSize, filter));
    }
}

public record GetTrainingByIdQuery(Guid Id) : IQuery<Result<Training>>;
public class GetTrainingByIdQueryHandler : BaseQueryHandler<GetTrainingByIdQuery, Result<Training>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetTrainingByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetTrainingByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<Training>> ExecuteAsync(GetTrainingByIdQuery request, CancellationToken ct) {
        var item = await _uow.Trainings.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<Training>("Not found");
    }
}
