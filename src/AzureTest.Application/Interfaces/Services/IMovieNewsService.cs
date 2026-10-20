using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Interfaces.Services;

public interface IMovieNewsService
{
    Task<IReadOnlyList<MovieNewsItemDto>> GetLatestNewsAsync(int count = 20, CancellationToken ct = default);
}
