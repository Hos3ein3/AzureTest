namespace AzureTest.Domain.Exceptions;

public sealed class UniqueConstraintViolationException : Exception
{
    public string? ConstraintName { get; }

    public UniqueConstraintViolationException(string? constraintName, Exception inner)
        : base($"Unique constraint '{constraintName}' was violated.", inner)
        => ConstraintName = constraintName;
}