using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AzureTest.MVC.Models;
using AzureTest.Application.Abstractions;

namespace AzureTest.MVC.Extensions;

public static class TempDataExtensions
{
    public static void SetToast(this ITempDataDictionary tempData, ResultStatus status, string message, string title = "")
    {
        var toast = new ToastMessage { Status = status, Message = message, Title = title };
        tempData["ToastMessage"] = toast.ToJson();
    }

    public static void SetToast(this ITempDataDictionary tempData, Result result, string successTitle = "Success", string errorTitle = "Error")
    {
        var title = result.IsSuccess ? successTitle : errorTitle;
        var message = result.Message ?? result.Error ?? (result.IsSuccess ? "Operation completed successfully." : "An error occurred.");
        
        if (result.IsFailure && result.ValidationErrors.Count > 0)
        {
            var firstErrorList = System.Linq.Enumerable.FirstOrDefault(result.ValidationErrors.Values);
            var firstError = firstErrorList != null && firstErrorList.Length > 0 ? firstErrorList[0] : null;
            if (!string.IsNullOrEmpty(firstError))
            {
                message = string.IsNullOrEmpty(result.Message) ? firstError : $"{message} - {firstError}";
            }
        }
        
        tempData.SetToast(result.Status, message, title);
    }
}
