using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetStatusLogsByResumeId;

public sealed record GetStatusLogsByResumeIdQuery(Guid ResumeId) : IQuery<IReadOnlyList<StatusLogDto>>;
