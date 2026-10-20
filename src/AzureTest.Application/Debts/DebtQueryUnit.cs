using System.Collections.Generic;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Debts.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Debts;

namespace AzureTest.Application.Debts;

public sealed class DebtQueryUnit : IDebtQueryUnit
{
    public IQueryHandler<GetAllDebtsQuery, PagedResult<DebtDto, GetAllDebtsFilters>> GetAll { get; }
    public IQueryHandler<GetDebtByIdQuery, DebtDto?> GetById { get; }
    public IQueryHandler<GetDebtAnalyticsQuery, DebtAnalyticsDto> GetAnalytics { get; }

    public DebtQueryUnit(
        IQueryHandler<GetAllDebtsQuery, PagedResult<DebtDto, GetAllDebtsFilters>> getAll,
        IQueryHandler<GetDebtByIdQuery, DebtDto?> getById,
        IQueryHandler<GetDebtAnalyticsQuery, DebtAnalyticsDto> getAnalytics)
    {
        GetAll = getAll;
        GetById = getById;
        GetAnalytics = getAnalytics;
    }
}
