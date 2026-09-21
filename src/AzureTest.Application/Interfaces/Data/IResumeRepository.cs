using System;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IResumeRepository : IGenericRepository<Resume>
{
    // Add any specific methods for Resume here, e.g., fetching by specific criteria
    Task<Resume?> GetResumeWithDetailsAsync(Guid id);
}
