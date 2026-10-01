using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Abstractions;

/// <summary>
/// Represents either a successful outcome or a failure with an error message.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(string error) => Result<TValue>.Failure(error);
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

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(string error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
