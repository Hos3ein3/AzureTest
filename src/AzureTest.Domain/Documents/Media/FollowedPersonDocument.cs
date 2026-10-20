using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Media;

public class FollowedPersonDocument : ITenantEntity
{
    public static class Rules
    {
        public const int NameMaxLength = 200;
        public const int DepartmentMaxLength = 100;
        public const int NotesMaxLength = 500;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }
    public int TmdbPersonId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string KnownForDepartment { get; private set; } = "Acting"; // "Acting", "Directing", etc.
    public string? ProfilePath { get; private set; }
    public string? Notes { get; private set; }
    public DateTime FollowedAt { get; private set; } = DateTime.UtcNow;

    private FollowedPersonDocument() { }

    public static FollowedPersonDocument Create(
        Guid userId,
        Guid tenantId,
        int tmdbPersonId,
        string name,
        string? department = "Acting",
        string? profilePath = null,
        string? notes = null)
    {
        var sanitizedName = string.IsNullOrWhiteSpace(name) ? "Unknown Person" : name.Trim();
        if (sanitizedName.Length > Rules.NameMaxLength)
        {
            sanitizedName = sanitizedName[..Rules.NameMaxLength];
        }

        var dept = string.IsNullOrWhiteSpace(department) ? "Acting" : department.Trim();
        if (dept.Length > Rules.DepartmentMaxLength)
        {
            dept = dept[..Rules.DepartmentMaxLength];
        }

        string? sanitizedNotes = null;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            sanitizedNotes = notes.Trim();
            if (sanitizedNotes.Length > Rules.NotesMaxLength)
            {
                sanitizedNotes = sanitizedNotes[..Rules.NotesMaxLength];
            }
        }

        return new FollowedPersonDocument
        {
            UserId = userId,
            TenantId = tenantId,
            TmdbPersonId = tmdbPersonId,
            Name = sanitizedName,
            KnownForDepartment = dept,
            ProfilePath = profilePath,
            Notes = sanitizedNotes,
            FollowedAt = DateTime.UtcNow
        };
    }

    public void UpdateNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            Notes = null;
            return;
        }

        var sanitized = notes.Trim();
        Notes = sanitized.Length > Rules.NotesMaxLength ? sanitized[..Rules.NotesMaxLength] : sanitized;
    }
}
