using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;

namespace AzureTest.Domain.Documents.Pages;

public class PageDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Pending;
    public List<Guid> LikedBy { get; set; } = new();
    public List<Guid> DislikedBy { get; set; } = new();
    public List<PageComment> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
