using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTest.Domain.Exceptions;

public class IdentityException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public IdentityException(IEnumerable<string> errors)
        : base(errors != null ? string.Join(", ", errors) : string.Empty)
    {
        Errors = errors?.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    public IdentityException(List<string> errors)
        : this((IEnumerable<string>)errors)
    {
    }

    public IdentityException(string message)
        : base(message)
    {
        Errors = new[] { message };
    }

    public IdentityException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = errors?.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
    }
}
