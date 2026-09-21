using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetAllResumeFiles;

public sealed record GetAllResumeFilesQuery : IQuery<IReadOnlyList<ResumeFileDto>>;
