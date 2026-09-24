using System;
using System.Collections.Generic;

namespace AzureTest.MVC.Models;

public class ResumeFileExplorerViewModel
{
    public bool IsAdmin { get; set; }
    public string CurrentFolder { get; set; } = string.Empty;
    public string CurrentFolderName { get; set; } = string.Empty;
    public List<UserFolderViewModel> AvailableFolders { get; set; } = new();
    public List<FileItemViewModel> Files { get; set; } = new();
}

public class UserFolderViewModel
{
    public string FolderId { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
}

public class FileItemViewModel
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    
    // DB mapping
    public Guid? DocumentId { get; set; }
    public Guid? ResumeId { get; set; }
    public string Type { get; set; } = string.Empty; // "CV" or "CL"
}
