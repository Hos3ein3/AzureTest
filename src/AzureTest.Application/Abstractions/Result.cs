using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Abstractions;

public enum ResultStatus
{
    Success,
    Info,
    Warning,
    Error
}

/// <summary>
/// Represents either a successful outcome or a failure with an error message.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = error;
        Status = isSuccess ? ResultStatus.Success : ResultStatus.Error;
    }

    protected Result(bool isSuccess, string? title, string? message, IDictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Title = title;
        Message = message;
        Error = message; // map for backwards compatibility
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        Status = isSuccess ? ResultStatus.Success : ResultStatus.Error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultStatus Status { get; protected set; }
    public string? Title { get; }
    public string? Message { get; }
    public IDictionary<string, string[]> ValidationErrors { get; } = new Dictionary<string, string[]>();

    public static Result Success() => new(true, null);
    public static Result Failure() => new(false, "non deterministic error happened");
    public static Result Failure(string error) => new(false, error);
    
    public static Result Failure(string title, string message) 
        => new(false, title, message, null);
        
    public static Result Failure(string title, string message, IDictionary<string, string[]> validationErrors) 
        => new(false, title, message, validationErrors);

    public static Result ValidationFailure(IDictionary<string, string[]> validationErrors, string title = "Validation Error", string message = "One or more validation errors occurred.")
        => new(false, title, message, validationErrors);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(string error) => Result<TValue>.Failure(error);
    
    public static Result<TValue> Failure<TValue>(string title, string message) 
        => Result<TValue>.Failure(title, message);
        
    public static Result<TValue> Failure<TValue>(string title, string message, IDictionary<string, string[]> validationErrors) 
        => Result<TValue>.Failure(title, message, validationErrors);

    public static Result<TValue> ValidationFailure<TValue>(IDictionary<string, string[]> validationErrors, string title = "Validation Error", string message = "One or more validation errors occurred.")
        => Result<TValue>.ValidationFailure(validationErrors, title, message);
}

/// <summary>
/// Represents either a successful outcome with a value, or a failure with an error message.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base(true, null)
    {
        _value = value;
    }

    private Result(string error) : base(false, error)
    {
        _value = default;
    }

    private Result(string title, string message, IDictionary<string, string[]>? validationErrors) 
        : base(false, title, message, validationErrors)
    {
        _value = default;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(string error) => new(error);
    
    public static Result<TValue> Failure(string title, string message) 
        => new(title, message, null);
        
    public static Result<TValue> Failure(string title, string message, IDictionary<string, string[]> validationErrors) 
        => new(title, message, validationErrors);

    public static Result<TValue> ValidationFailure(IDictionary<string, string[]> validationErrors, string title = "Validation Error", string message = "One or more validation errors occurred.")
        => new(title, message, validationErrors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
