using System.Collections.Generic;
using AzureTest.Application.Debts.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Debts;

namespace AzureTest.Application.Debts.Interfaces;

public interface IDebtQueryUnit
{
    IQueryHandler<GetAllDebtsQuery, IReadOnlyList<DebtDto>> GetAll { get; }
    IQueryHandler<GetDebtByIdQuery, DebtDto?> GetById { get; }
}
