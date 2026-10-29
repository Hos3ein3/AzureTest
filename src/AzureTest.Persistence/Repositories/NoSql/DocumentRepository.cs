

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Primitives;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class DocumentRepository<TDocument> : IDocumentRepository<TDocument> where TDocument : class
{
    protected readonly IMongoCollection<TDocument> _collection;
    protected readonly ITenantProvider? _tenantProvider;

    public DocumentRepository(IMongoDatabase database, string collectionName, ITenantProvider? tenantProvider = null)
    {
        _collection = database.GetCollection<TDocument>(collectionName);
        _tenantProvider = tenantProvider;
    }

    protected virtual FilterDefinition<TDocument> GetBaseFilter()
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var filters = new List<FilterDefinition<TDocument>>();

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TDocument)))
        {
            filters.Add(filterBuilder.Eq("IsDeleted", false));
        }

        if (_tenantProvider?.IsMultiTenancyEnabled == true &&
            _tenantProvider.CurrentTenantId != Guid.Empty &&
            typeof(ITenantEntity).IsAssignableFrom(typeof(TDocument)))
        {
            var hasUserId = typeof(TDocument).GetProperty("UserId") != null;
            if (hasUserId)
            {
                // Match TenantId OR fallback to UserId for legacy database records created prior to multi-tenancy
                filters.Add(filterBuilder.Or(
                    filterBuilder.Eq("TenantId", _tenantProvider.CurrentTenantId),
                    filterBuilder.And(
                        filterBuilder.Or(
                            filterBuilder.Eq("TenantId", Guid.Empty),
                            filterBuilder.Exists("TenantId", false)
                        ),
                        filterBuilder.Eq("UserId", _tenantProvider.CurrentTenantId)
                    )
                ));
            }
            else
            {
                filters.Add(filterBuilder.Eq("TenantId", _tenantProvider.CurrentTenantId));
            }
        }

        return filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
    }

    public virtual async Task<TDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var filter = filterBuilder.Eq("_id", id);
        var baseFilter = GetBaseFilter();
        if (baseFilter != filterBuilder.Empty)
        {
            filter &= baseFilter;
        }
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filter = GetBaseFilter();
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TDocument>> GetAllAvailableAsync()
    {
        var filter = GetBaseFilter();
        return await _collection.Find(filter).ToListAsync();
    }

    public virtual async Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate)
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var predFilter = filterBuilder.Where(predicate);
        var baseFilter = GetBaseFilter();
        var combined = baseFilter != filterBuilder.Empty ? (baseFilter & predFilter) : predFilter;
        return await _collection.Find(combined).ToListAsync();
    }

    public virtual async Task AddAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        if (_tenantProvider?.IsMultiTenancyEnabled == true &&
            _tenantProvider.CurrentTenantId != Guid.Empty &&
            document is ITenantEntity tenantDoc &&
            tenantDoc.TenantId == Guid.Empty)
        {
            tenantDoc.TenantId = _tenantProvider.CurrentTenantId;
        }

        var effectiveUserId = _tenantProvider?.IsAdminImpersonating == true
            ? _tenantProvider.CurrentTenantId
            : (_tenantProvider?.RealUserId ?? Guid.Empty);

        var userIdProp = typeof(TDocument).GetProperty("UserId");
        if (userIdProp != null && userIdProp.CanWrite && effectiveUserId != Guid.Empty)
        {
            var currentUserId = userIdProp.GetValue(document) as Guid?;
            if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty || (_tenantProvider?.IsAdminImpersonating == true && currentUserId.Value == _tenantProvider.RealUserId))
            {
                userIdProp.SetValue(document, effectiveUserId);
            }
        }

        if (document is ISingleRecordSetting)
        {
            var baseFilter = GetBaseFilter();
            var existingCount = await _collection.CountDocumentsAsync(baseFilter, cancellationToken: cancellationToken);
            if (existingCount > 0)
            {
                throw new InvalidOperationException($"An instance of single-record setting '{typeof(TDocument).Name}' already exists. Only one record is permitted.");
            }
        }

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public virtual async Task UpdateAsync(Guid id, TDocument document, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false }, cancellationToken);
    }

    public virtual async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public virtual async Task<(IReadOnlyList<TDocument> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TDocument, bool>>? predicate,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var filter = predicate != null ? filterBuilder.Where(predicate) : filterBuilder.Empty;

        if (typeof(AzureTest.Domain.Primitives.ISoftDelete).IsAssignableFrom(typeof(TDocument)))
        {
            var isDeletedProp = typeof(TDocument).GetProperty("IsDeleted");
            if (isDeletedProp != null)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq("IsDeleted", false));
            }
        }

        if (_tenantProvider?.IsMultiTenancyEnabled == true &&
            _tenantProvider.CurrentTenantId != Guid.Empty &&
            typeof(AzureTest.Domain.Primitives.ITenantEntity).IsAssignableFrom(typeof(TDocument)))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq("TenantId", _tenantProvider.CurrentTenantId));
        }

        var totalCount = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var findFluent = _collection.Find(filter);

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var sort = sortDescending
                ? Builders<TDocument>.Sort.Descending(sortBy)
                : Builders<TDocument>.Sort.Ascending(sortBy);
            findFluent = findFluent.Sort(sort);
        }

        int page = pageNumber < 1 ? 1 : pageNumber;
        int size = pageSize < 1 ? 10 : pageSize;

        if (pageSize > 0)
        {
            findFluent = findFluent.Skip((page - 1) * size).Limit(size);
        }

        var items = await findFluent.ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public virtual async Task<PagedResult<TDocument>> GetPagedAsync(
        IPagedFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await ExecutePagedFilterAsync(filter, predicate, cancellationToken);
        int page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        int size = filter.PageSize < 1 ? 10 : filter.PageSize;
        return new PagedResult<TDocument>(items, totalCount, page, size, filter);
    }

    public virtual async Task<PagedResult<TDocument, TFilter>> GetPagedAsync<TFilter>(
        TFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TFilter : class, IPagedFilter
    {
        var (items, totalCount) = await ExecutePagedFilterAsync(filter, predicate, cancellationToken);
        int page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        int size = filter.PageSize < 1 ? 10 : filter.PageSize;
        return new PagedResult<TDocument, TFilter>(items, totalCount, page, size, filter);
    }

    protected virtual async Task<(IReadOnlyList<TDocument> Items, int TotalCount)> ExecutePagedFilterAsync(
        IPagedFilter filter,
        Expression<Func<TDocument, bool>>? predicate,
        CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var filtersList = new List<FilterDefinition<TDocument>>();

        if (typeof(AzureTest.Domain.Primitives.ISoftDelete).IsAssignableFrom(typeof(TDocument)))
        {
            var isDeletedProp = typeof(TDocument).GetProperty("IsDeleted");
            if (isDeletedProp != null)
            {
                filtersList.Add(filterBuilder.Eq("IsDeleted", false));
            }
        }

        if (_tenantProvider?.IsMultiTenancyEnabled == true &&
            _tenantProvider.CurrentTenantId != Guid.Empty &&
            typeof(AzureTest.Domain.Primitives.ITenantEntity).IsAssignableFrom(typeof(TDocument)))
        {
            filtersList.Add(filterBuilder.Eq("TenantId", _tenantProvider.CurrentTenantId));
        }

        if (predicate != null)
        {
            filtersList.Add(filterBuilder.Where(predicate));
        }

        if (filter.UserIds != null && filter.UserIds.Any())
        {
            var userIdProp = typeof(TDocument).GetProperty("UserId");
            if (userIdProp != null)
            {
                filtersList.Add(filterBuilder.In("UserId", filter.UserIds));
            }
        }

        if (filter.CreatedOnFrom.HasValue || filter.CreatedOnTo.HasValue)
        {
            var dateProp = typeof(TDocument).GetProperty("CreatedOn")
                        ?? typeof(TDocument).GetProperty("CreatedAt")
                        ?? typeof(TDocument).GetProperty("Date");
            if (dateProp != null)
            {
                if (filter.CreatedOnFrom.HasValue)
                {
                    filtersList.Add(filterBuilder.Gte(dateProp.Name, filter.CreatedOnFrom.Value));
                }
                if (filter.CreatedOnTo.HasValue)
                {
                    var toDate = filter.CreatedOnTo.Value.TimeOfDay == TimeSpan.Zero
                        ? filter.CreatedOnTo.Value.Date.AddDays(1).AddTicks(-1)
                        : filter.CreatedOnTo.Value;
                    filtersList.Add(filterBuilder.Lte(dateProp.Name, toDate));
                }
            }
        }

        var combinedFilter = filtersList.Any() ? filterBuilder.And(filtersList) : filterBuilder.Empty;
        var totalCount = (int)await _collection.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

        var findFluent = _collection.Find(combinedFilter);

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var sort = filter.SortDescending
                ? Builders<TDocument>.Sort.Descending(filter.SortBy)
                : Builders<TDocument>.Sort.Ascending(filter.SortBy);
            findFluent = findFluent.Sort(sort);
        }

        int page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        int size = filter.PageSize < 1 ? 10 : filter.PageSize;

        if (filter.PageSize > 0)
        {
            findFluent = findFluent.Skip((page - 1) * size).Limit(size);
        }

        var items = await findFluent.ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
