namespace AzureTest.MVC.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? ExceptionMessage { get; set; }
    public string? InnerExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
}
