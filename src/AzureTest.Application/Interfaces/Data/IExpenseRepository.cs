using System;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IExpenseRepository : IGenericRepository<Expense>
{
    // Add any specific methods for Expense here if needed
}
