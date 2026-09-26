using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetResumeFileById;

internal sealed class GetResumeFileByIdQueryHandler : IQueryHandler<GetResumeFileByIdQuery, ResumeFileDto?>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetResumeFileByIdQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<ResumeFileDto?> HandleAsync(GetResumeFileByIdQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _noSqlUnitOfWork.ResumeFiles.GetByIdAsync(query.Id);

        if (document is null) return null;

        return new ResumeFileDto(
            document.Id,
            document.ResumeId,
            document.RealNameOfCV,
            document.TempNameOfCV,
            document.RealNameOfCL,
            document.TempNameOfCL,
            document.CvSize,
            document.ClSize,
            document.UploadedOn,
            document.UserId
        );
    }
}
