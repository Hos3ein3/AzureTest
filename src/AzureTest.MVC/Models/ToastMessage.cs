using AzureTest.Application.Abstractions;
using System.Text.Json;

namespace AzureTest.MVC.Models;

public class ToastMessage
{
    public ResultStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public static ToastMessage? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<ToastMessage>(json); }
        catch { return null; }
    }
    
    public string ToJson() => JsonSerializer.Serialize(this);
}
