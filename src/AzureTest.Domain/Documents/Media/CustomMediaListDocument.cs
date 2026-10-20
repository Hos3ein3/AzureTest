using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.Domain.Media.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Media;

public class CustomMediaListItem
{
    public int TmdbId { get; set; }
    public MediaType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public int? Year { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class CustomMediaListDocument : ITenantEntity
{
    public static class Rules
    {
        public const int TitleMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int MaxItems = 300;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public List<CustomMediaListItem> Items { get; private set; } = new();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private CustomMediaListDocument() { }

    public static CustomMediaListDocument Create(
        Guid userId,
        Guid tenantId,
        string title,
        string? description = null)
    {
        var sanitizedTitle = string.IsNullOrWhiteSpace(title) ? "New List" : title.Trim();
        if (sanitizedTitle.Length > Rules.TitleMaxLength)
        {
            sanitizedTitle = sanitizedTitle[..Rules.TitleMaxLength];
        }

        string? sanitizedDesc = null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            sanitizedDesc = description.Trim();
            if (sanitizedDesc.Length > Rules.DescriptionMaxLength)
            {
                sanitizedDesc = sanitizedDesc[..Rules.DescriptionMaxLength];
            }
        }

        return new CustomMediaListDocument
        {
            UserId = userId,
            TenantId = tenantId,
            Title = sanitizedTitle,
            Description = sanitizedDesc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool AddItem(int tmdbId, MediaType type, string title, string? posterPath = null, int? year = null)
    {
        if (Items.Count >= Rules.MaxItems) return false;
        if (Items.Any(i => i.TmdbId == tmdbId && i.Type == type)) return false;

        Items.Add(new CustomMediaListItem
        {
            TmdbId = tmdbId,
            Type = type,
            Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title.Trim(),
            PosterPath = posterPath,
            Year = year,
            AddedAt = DateTime.UtcNow
        });

        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool RemoveItem(int tmdbId, MediaType type)
    {
        var removed = Items.RemoveAll(i => i.TmdbId == tmdbId && i.Type == type) > 0;
        if (removed)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        return removed;
    }

    public void UpdateDetails(string title, string? description)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            var t = title.Trim();
            Title = t.Length > Rules.TitleMaxLength ? t[..Rules.TitleMaxLength] : t;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            var d = description.Trim();
            Description = d.Length > Rules.DescriptionMaxLength ? d[..Rules.DescriptionMaxLength] : d;
        }
        else
        {
            Description = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
