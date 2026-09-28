using System;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories;

public class DebtRepository : GenericRepository<Debt>, IDebtRepository
{
    public DebtRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
