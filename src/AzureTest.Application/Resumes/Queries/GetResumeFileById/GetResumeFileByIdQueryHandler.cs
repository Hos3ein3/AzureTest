using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetResumeFileById;

internal sealed class GetResumeFileByIdQueryHandler : IQueryHandler<GetResumeFileByIdQuery, ResumeFileDto?>
{
    private readonly IResumeFilesRepository _resumeFilesRepository;

    public GetResumeFileByIdQueryHandler(IResumeFilesRepository resumeFilesRepository)
    {
        _resumeFilesRepository = resumeFilesRepository;
    }

    public async Task<ResumeFileDto?> HandleAsync(GetResumeFileByIdQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _resumeFilesRepository.GetByIdAsync(query.Id);

        if (document is null) return null;

        return new ResumeFileDto(
            document.Id,
            document.ResumeId,
            document.RealNameOfCV,
            document.TempNameOfCV,
            document.RealNameOfCL,
            document.TempNameOfCL,
            document.UploadedOn
        );
    }
}
