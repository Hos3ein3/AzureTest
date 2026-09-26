using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetResumeStatusLogsByResumeId;

public sealed record GetResumeStatusLogsByResumeIdQuery(Guid ResumeId) : IQuery<IReadOnlyList<ResumeStatusLogDto>>;
