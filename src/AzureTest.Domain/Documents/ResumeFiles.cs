using System;

namespace AzureTest.Domain.Documents;

public class ResumeFiles
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Foreign key to the SQL Resume Aggregate Root (1-1 relationship)
    /// </summary>
    public Guid ResumeId { get; set; }

    // Curriculum Vitae (CV) Properties
    public string? RealNameOfCV { get; set; }
    public string? TempNameOfCV { get; set; }

    // Cover Letter (CL) Properties
    public string? RealNameOfCL { get; set; }
    public string? TempNameOfCL { get; set; }

    // Additional properties
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
}