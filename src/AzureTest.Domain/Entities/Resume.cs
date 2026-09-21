using System;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public sealed class Resume : AggregateRoot, IAudit, ISoftDelete
{
    public string PositionTitle { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string JobLink { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public ResumeSource Source { get; private set; }
    public ResumeStatus Status { get; private set; }

    // IAudit properties
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // Parameterless constructor for EF Core
    private Resume() { }

    private Resume(
        string positionTitle, 
        string description, 
        string location, 
        string jobLink, 
        string note, 
        ResumeSource source, 
        ResumeStatus status)
    {
        PositionTitle = positionTitle;
        Description = description;
        Location = location;
        JobLink = jobLink;
        Note = note;
        Source = source;
        Status = status;
    }

    public static Resume Create(
        string positionTitle, 
        string description, 
        string location, 
        string jobLink, 
        string note, 
        ResumeSource source, 
        ResumeStatus status = ResumeStatus.Applied)
    {
        return new Resume(
            positionTitle,
            description,
            location,
            jobLink,
            note,
            source,
            status
        );
    }

    public void UpdateStatus(ResumeStatus newStatus)
    {
        Status = newStatus;
    }
    
    public void UpdateDetails(
        string positionTitle, 
        string description, 
        string location, 
        string jobLink, 
        string note)
    {
        PositionTitle = positionTitle;
        Description = description;
        Location = location;
        JobLink = jobLink;
        Note = note;
    }
}