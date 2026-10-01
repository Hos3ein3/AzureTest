

using System;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IDebtRepository : IGenericRepository<Debt>
{
}
