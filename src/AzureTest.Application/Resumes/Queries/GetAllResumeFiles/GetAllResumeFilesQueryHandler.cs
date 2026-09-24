using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetAllResumeFiles;


public sealed record GetAllResumeFilesQuery : IQuery<IReadOnlyList<ResumeFileDto>>;

internal sealed class GetAllResumeFilesQueryHandler : IQueryHandler<GetAllResumeFilesQuery, IReadOnlyList<ResumeFileDto>>
{
    private readonly IResumeFilesRepository _resumeFilesRepository;

    public GetAllResumeFilesQueryHandler(IResumeFilesRepository resumeFilesRepository)
    {
        _resumeFilesRepository = resumeFilesRepository;
    }

    public async Task<IReadOnlyList<ResumeFileDto>> HandleAsync(GetAllResumeFilesQuery query, CancellationToken cancellationToken = default)
    {
        var documents = await _resumeFilesRepository.GetAllAvailableAsync();

        return documents.Select(d => new ResumeFileDto(
            d.Id,
            d.ResumeId,
            d.RealNameOfCV,
            d.TempNameOfCV,
            d.RealNameOfCL,
            d.TempNameOfCL,
            d.CvSize,
            d.ClSize,
            d.UploadedOn,
            d.UserId
        )).ToList();
    }
}
