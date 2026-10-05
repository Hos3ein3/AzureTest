using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetAllResumeFiles;
using AzureTest.Application.Resumes.Queries.GetResumeFileById;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Linq;
using AzureTest.Domain.Entities.Identity;
using AzureTest.MVC.Models;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Resume File")]
[Description("Manage and control Resume File operations.")]
    public class ResumeFileController : Controller
{
    private readonly IResumeFileQueryUnit _queries;
    private readonly IResumeDocumentService _documentService;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResumeFileController(
        IResumeFileQueryUnit queries,
        IResumeDocumentService documentService,
        IWebHostEnvironment env,
        ICurrentUser currentUser,
        UserManager<ApplicationUser> userManager)
    {
        _queries = queries;
        _documentService = documentService;
        _env = env;
        _currentUser = currentUser;
        _userManager = userManager;
    }

    // GET: /ResumeFile
    [HttpGet]
    [DisplayName("Index")]
    [Description("View a list of Resume File.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var allFiles = await _queries.GetAll.HandleAsync(new GetAllResumeFilesQuery(), cancellationToken);
        var userFiles = allFiles.Where(f => f.UserId == _currentUser.Id).ToList();
        return View(userFiles);
    }

    // GET: /ResumeFile/Download/{id}?type=cv|cl
    [HttpGet]
    [DisplayName("Download")]
    [Description("Execute Download operation for Resume File.")]
    public async Task<IActionResult> Download(Guid id, string type, CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadFileAsync(id, type, cancellationToken);
        if (result is null) return NotFound("File not found.");

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    // GET: /ResumeFile/Delete/{id}
    [HttpGet]
    [DisplayName("Delete")]
    [Description("Delete a Resume File.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var fileRecord = await _queries.GetById.HandleAsync(new GetResumeFileByIdQuery(id), cancellationToken);
        if (fileRecord is null) return NotFound();
        return View(fileRecord);
    }

    // POST: /ResumeFile/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Confirmed")]
    [Description("Delete a Resume File.")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var fileRecord = await _queries.GetById.HandleAsync(new GetResumeFileByIdQuery(id), cancellationToken);
        if (fileRecord is not null)
        {
            await _documentService.DeleteAllFilesByResumeIdAsync(fileRecord.ResumeId, cancellationToken);
            TempData["Success"] = "Resume files deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: /ResumeFile/SyncSizes
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Sync Sizes")]
    [Description("Execute Sync Sizes operation for Resume File.")]
    public async Task<IActionResult> SyncSizes(CancellationToken cancellationToken)
    {
        int count = await _documentService.SyncAllFileSizesAsync(cancellationToken);
        TempData["Success"] = $"Successfully synchronized sizes for {count} files.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /ResumeFile/Explorer
    [HttpGet]
    [DisplayName("Explorer")]
    [Description("Execute Explorer operation for Resume File.")]
    public async Task<IActionResult> Explorer(string? folder, CancellationToken cancellationToken)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("ResumeFile/Delete");
        string currentFolder = isAdmin && !string.IsNullOrWhiteSpace(folder) 
            ? folder 
            : _currentUser.Id.ToString();

        var model = new ResumeFileExplorerViewModel
        {
            IsAdmin = isAdmin,
            CurrentFolder = currentFolder
        };

        var rootPath = Path.Combine(_env.WebRootPath, "uploads", "resumes");
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

        if (isAdmin)
        {
            var directories = Directory.GetDirectories(rootPath);
            foreach (var d in directories)
            {
                var folderName = Path.GetFileName(d);
                var user = await _userManager.FindByIdAsync(folderName);
                var displayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : folderName;
                if (string.IsNullOrWhiteSpace(displayName)) displayName = user?.Email ?? folderName;
                
                model.AvailableFolders.Add(new UserFolderViewModel
                {
                    FolderId = folderName,
                    FolderName = displayName
                });
                
                if (currentFolder == folderName)
                {
                    model.CurrentFolderName = displayName;
                }
            }
        }
        else
        {
            model.CurrentFolderName = _currentUser.FullName;
        }

        var folderPath = Path.Combine(rootPath, currentFolder);
        if (Directory.Exists(folderPath))
        {
            var allDbFiles = await _queries.GetAll.HandleAsync(new GetAllResumeFilesQuery(), cancellationToken);
            
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var size = new FileInfo(file).Length;
                
                var matchedDb = allDbFiles.FirstOrDefault(d => d.TempNameOfCV == fileName || d.TempNameOfCL == fileName);
                
                model.Files.Add(new FileItemViewModel
                {
                    FileName = fileName,
                    SizeBytes = size,
                    RelativePath = $"/uploads/resumes/{currentFolder}/{fileName}",
                    DocumentId = matchedDb?.Id,
                    ResumeId = matchedDb?.ResumeId,
                    Type = matchedDb?.TempNameOfCV == fileName ? "cv" : (matchedDb?.TempNameOfCL == fileName ? "cl" : "")
                });
            }
        }

        return View(model);
    }

    // GET: /ResumeFile/Preview
    [HttpGet]
    [DisplayName("Preview")]
    [Description("View details of a Resume File.")]
    public IActionResult Preview(string folder, string fileName)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("ResumeFile/Delete");
        if (!isAdmin && folder != _currentUser.Id.ToString())
            return Forbid();

        ViewBag.FileUrl = $"/uploads/resumes/{folder}/{fileName}";
        ViewBag.FileName = fileName;
        return View();
    }

    // POST: /ResumeFile/DeletePhysicalFile
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Physical File")]
    [Description("Delete a Resume File.")]
    public async Task<IActionResult> DeletePhysicalFile(string folder, string fileName, Guid? documentId, string type, CancellationToken cancellationToken)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("ResumeFile/Delete");
        if (!isAdmin && folder != _currentUser.Id.ToString())
            return Forbid();

        if (documentId.HasValue && !string.IsNullOrWhiteSpace(type))
        {
            await _documentService.DeleteFileAsync(documentId.Value, type, cancellationToken);
        }
        else
        {
            var path = Path.Combine(_env.WebRootPath, "uploads", "resumes", folder, fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        TempData["Success"] = "File deleted successfully.";
        return RedirectToAction(nameof(Explorer), new { folder = isAdmin ? folder : null });
    }
}
