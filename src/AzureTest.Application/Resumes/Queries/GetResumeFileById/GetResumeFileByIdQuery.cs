using System;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes.Queries.GetResumeFileById;

public sealed record GetResumeFileByIdQuery(Guid Id) : IQuery<ResumeFileDto?>;
