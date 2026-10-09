using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Pages;

public class PageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public PageStatus Status { get; set; }
    public List<Guid> LikedBy { get; set; } = new();
    public List<Guid> DislikedBy { get; set; } = new();
    public List<PageCommentDto> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePageDto : IPageFormDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
}

public class UpdatePageDto : IPageFormDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public PageStatus Status { get; set; }
}

public class PageCommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AdminReply { get; set; }
    public DateTime? AdminReplyAt { get; set; }
}

public interface IPageFormDto
{
    string Title { get; set; }
    string Slug { get; set; }
    string Content { get; set; }
    string? ImagePath { get; set; }
}
