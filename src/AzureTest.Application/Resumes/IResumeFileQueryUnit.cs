using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumeFiles;
using AzureTest.Application.Resumes.Queries.GetResumeFileById;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes;

public interface IResumeFileQueryUnit
{
    IQueryHandler<GetAllResumeFilesQuery, IReadOnlyList<ResumeFileDto>> GetAll { get; }
    IQueryHandler<GetResumeFileByIdQuery, ResumeFileDto?> GetById { get; }
}
