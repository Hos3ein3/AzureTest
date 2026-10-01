

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Companies;

namespace AzureTest.Application.Interfaces.Services;

public interface ICompanyCacheService
{
    Task<IEnumerable<CompanyDto>?> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task SetCompaniesAsync(IEnumerable<CompanyDto> companies, CancellationToken cancellationToken = default);
    Task InvalidateCompaniesAsync(CancellationToken cancellationToken = default);
}
