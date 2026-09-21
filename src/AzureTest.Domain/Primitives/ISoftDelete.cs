using System;

namespace AzureTest.Domain.Primitives;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOn { get; set; }
    string? DeletedBy { get; set; }
}
