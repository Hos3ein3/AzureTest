using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumeFiles;
using AzureTest.Application.Resumes.Queries.GetResumeFileById;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application.Resumes;

internal sealed class ResumeFileQueryUnit : IResumeFileQueryUnit
{
    private readonly IServiceProvider _serviceProvider;

    public ResumeFileQueryUnit(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IQueryHandler<GetAllResumeFilesQuery, IReadOnlyList<ResumeFileDto>> GetAll => 
        _serviceProvider.GetRequiredService<IQueryHandler<GetAllResumeFilesQuery, IReadOnlyList<ResumeFileDto>>>();

    public IQueryHandler<GetResumeFileByIdQuery, ResumeFileDto?> GetById => 
        _serviceProvider.GetRequiredService<IQueryHandler<GetResumeFileByIdQuery, ResumeFileDto?>>();
}
