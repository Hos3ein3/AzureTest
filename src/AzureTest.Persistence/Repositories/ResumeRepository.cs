using System;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class ResumeRepository : GenericRepository<Resume>, IResumeRepository
{
    public ResumeRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Resume?> GetResumeWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
