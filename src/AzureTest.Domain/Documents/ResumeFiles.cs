using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class ResumeFiles : ISoftDelete
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Foreign key to the SQL Resume Aggregate Root (1-1 relationship)
    /// </summary>
    public Guid ResumeId { get; init; }
    
    public Guid UserId { get; set; }

    // Curriculum Vitae (CV) Properties
    public string? RealNameOfCV { get; set; }
    public string? TempNameOfCV { get; set; }
    public long CvSize { get; set; }

    // Cover Letter (CL) Properties
    public string? RealNameOfCL { get; set; }
    public string? TempNameOfCL { get; set; }
    public long ClSize { get; set; }

    // Additional properties
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

    // ISoftDelete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}