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

public class TrainingCourseFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public TrainingCourseFilter()
    {
        SortBy = "Name";
        SortDescending = false;
        PageSize = 10;
    }
}

public record GetAllTrainingCoursesQuery(TrainingCourseFilter? Filter = null) : IQuery<Result<AzureTest.Contracts.DTOs.Common.PagedResult<TrainingCourse, TrainingCourseFilter>>>;
public class GetAllTrainingCoursesQueryHandler : BaseQueryHandler<GetAllTrainingCoursesQuery, Result<AzureTest.Contracts.DTOs.Common.PagedResult<TrainingCourse, TrainingCourseFilter>>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetAllTrainingCoursesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllTrainingCoursesQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<AzureTest.Contracts.DTOs.Common.PagedResult<TrainingCourse, TrainingCourseFilter>>> ExecuteAsync(GetAllTrainingCoursesQuery request, CancellationToken ct) {
        var filter = request.Filter ?? new TrainingCourseFilter();
        var items = await _uow.TrainingCourses.GetAllAsync();
        var query = items.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(x => x.Name != null && x.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        bool desc = filter.SortDescending;
        query = desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

        var list = query.ToList();
        int totalItems = list.Count;
        int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
        int currentPage = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var pagedList = (filter.PageSize <= 0)
            ? list
            : list.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        return Result.Success(new AzureTest.Contracts.DTOs.Common.PagedResult<TrainingCourse, TrainingCourseFilter>(pagedList, totalItems, currentPage, pageSize, filter));
    }
}

public record GetTrainingCourseByIdQuery(Guid Id) : IQuery<Result<TrainingCourse>>;
public class GetTrainingCourseByIdQueryHandler : BaseQueryHandler<GetTrainingCourseByIdQuery, Result<TrainingCourse>> {
    private readonly INoSqlUnitOfWork _uow;
    public GetTrainingCourseByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetTrainingCourseByIdQueryHandler> logger) : base(logger) => _uow = uow;
    protected override async Task<Result<TrainingCourse>> ExecuteAsync(GetTrainingCourseByIdQuery request, CancellationToken ct) {
        var item = await _uow.TrainingCourses.GetByIdAsync(request.Id);
        return item != null ? Result.Success(item) : Result.Failure<TrainingCourse>("Not found");
    }
}
