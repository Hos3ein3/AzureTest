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
    public string? Note { get; private set; } = string.Empty;
    public ResumeSource Source { get; private set; }
    public ResumeStatus Status { get; private set; }
    public int? AtsScore { get; private set; }

    // IAudit properties
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    public string? ChatUrl { get; set; } = null;
    
    public Guid? CompanyId { get; set; }

    public Guid UserId { get; private set; }



    // Parameterless constructor for EF Core
    private Resume() { }

    private Resume(
        string positionTitle, 
        string description, 
        string location, 
        string jobLink, 
        string note, 
        string chatUrl,
        Guid? companyId,
        Guid userId,
        ResumeSource source, 
        ResumeStatus status,
        int? atsScore = null)
    {
        PositionTitle = positionTitle;
        Description = description;
        Location = location;
        JobLink = jobLink;
        Note = note;
        Source = source;
        Status = status;
        ChatUrl = chatUrl;
        CompanyId = companyId;
        UserId = userId;
        AtsScore = atsScore;
    }

    public static Resume Create(
        string positionTitle, 
        string description, 
        string location, 
        string jobLink, 
        string note, 
        string chatUrl,
        Guid? companyId,
        Guid userId,
        ResumeSource source, 
        ResumeStatus status = ResumeStatus.Applied,
        int? atsScore = null)
    {
        return new Resume(
            positionTitle,
            description,
            location,
            jobLink,
            note,
            chatUrl,
            companyId,
            userId,
            source,
            status,
            atsScore
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
        Guid? companyId,
        string note,string chatUrl, int? atsScore)
    {
        PositionTitle = positionTitle;
        Description = description;
        Location = location;
        JobLink = jobLink;
        Note = note;
        ChatUrl = chatUrl;
        AtsScore = atsScore;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}