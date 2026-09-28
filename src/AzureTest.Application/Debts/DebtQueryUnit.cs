using System.Collections.Generic;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Debts.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Debts;

namespace AzureTest.Application.Debts;

public sealed class DebtQueryUnit : IDebtQueryUnit
{
    public IQueryHandler<GetAllDebtsQuery, IReadOnlyList<DebtDto>> GetAll { get; }
    public IQueryHandler<GetDebtByIdQuery, DebtDto?> GetById { get; }

    public DebtQueryUnit(
        IQueryHandler<GetAllDebtsQuery, IReadOnlyList<DebtDto>> getAll,
        IQueryHandler<GetDebtByIdQuery, DebtDto?> getById)
    {
        GetAll = getAll;
        GetById = getById;
    }
}
