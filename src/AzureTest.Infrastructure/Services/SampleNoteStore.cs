using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

/// <summary>
/// Dev-only singleton in-memory store for SampleNote, backed by a pipe-delimited flat file.
/// </summary>
public sealed class SampleNoteStore : ISampleNoteStore
{
    private readonly List<SampleNote> _notes = new();
    private readonly string _filePath;
    private readonly ILogger<SampleNoteStore> _logger;
    private readonly object _lock = new();

    public SampleNoteStore(ILogger<SampleNoteStore> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(AppContext.BaseDirectory, "sample_notes_data.csv");
        LoadFromFile();
    }

    public IReadOnlyList<SampleNote> GetAll()
    {
        lock (_lock) return _notes.ToList().AsReadOnly();
    }

    public SampleNote? GetById(Guid id)
    {
        lock (_lock) return _notes.FirstOrDefault(n => n.Id == id);
    }

    public void Add(SampleNote note)
    {
        lock (_lock)
        {
            _notes.Add(note);
            SaveToFile();
        }
    }

    public bool Update(SampleNote note)
    {
        lock (_lock)
        {
            var idx = _notes.FindIndex(n => n.Id == note.Id);
            if (idx < 0) return false;
            _notes[idx] = note;
            SaveToFile();
            return true;
        }
    }

    public bool Delete(Guid id)
    {
        lock (_lock)
        {
            var removed = _notes.RemoveAll(n => n.Id == id);
            if (removed > 0) SaveToFile();
            return removed > 0;
        }
    }

    // ── File I/O ──────────────────────────────────────────────────────────────
    private void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath)) return;
            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines.Skip(1)) // skip header
            {
                var note = ParseLine(line);
                if (note != null) _notes.Add(note);
            }
            _logger.LogInformation("SampleNoteStore: loaded {Count} notes from {Path}", _notes.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SampleNoteStore: failed to load from file, starting empty.");
        }
    }

    private void SaveToFile()
    {
        try
        {
            var lines = new List<string> { "Id|Title|Content|Category|Priority|IsPinned|CreatedAt|UpdatedAt" };
            lines.AddRange(_notes.Select(SerializeLine));
            File.WriteAllLines(_filePath, lines);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SampleNoteStore: failed to persist to file.");
        }
    }

    private static string SerializeLine(SampleNote n) =>
        $"{n.Id}|{Escape(n.Title)}|{Escape(n.Content)}|{(int)n.Category}|{n.Priority}|{n.IsPinned}|{n.CreatedAt:O}|{n.UpdatedAt?.ToString("O") ?? ""}";

    private static SampleNote? ParseLine(string line)
    {
        try
        {
            var p = line.Split('|');
            if (p.Length < 8) return null;
            return new SampleNote
            {
                Title = Unescape(p[1]),
                Content = Unescape(p[2]),
                Category = (NoteCategory)int.Parse(p[3]),
                Priority = int.Parse(p[4]),
                IsPinned = bool.Parse(p[5]),
                CreatedAt = DateTime.Parse(p[6]),
                UpdatedAt = string.IsNullOrEmpty(p[7]) ? null : DateTime.Parse(p[7])
            };
            // Note: Id is set by BaseEntity constructor, we override it here
        }
        catch { return null; }
    }

    private static string Escape(string s) => s.Replace("|", "\\|").Replace("\n", "\\n").Replace("\r", "");
    private static string Unescape(string s) => s.Replace("\\|", "|").Replace("\\n", "\n");
}
