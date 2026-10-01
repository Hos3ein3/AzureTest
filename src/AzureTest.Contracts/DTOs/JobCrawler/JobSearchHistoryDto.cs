using System;
namespace AzureTest.Contracts.DTOs.JobCrawler;

public record JobSearchHistoryDto(
    string Id,
    Guid UserId,
    string Position,
    string Location,
    string DatePosted,
    string Source,
    DateTime SearchDate
);
