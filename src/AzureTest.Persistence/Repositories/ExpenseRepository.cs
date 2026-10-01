

using System;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }
}
