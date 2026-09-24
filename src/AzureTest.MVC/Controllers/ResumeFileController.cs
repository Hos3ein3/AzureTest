using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetAllResumeFiles;
using AzureTest.Application.Resumes.Queries.GetResumeFileById;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

public class ResumeFileController : Controller
{
    private readonly IResumeFileQueryUnit _queries;
    private readonly IResumeDocumentService _documentService;

    public ResumeFileController(
        IResumeFileQueryUnit queries,
        IResumeDocumentService documentService)
    {
        _queries = queries;
        _documentService = documentService;
    }

    // GET: /ResumeFile
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var files = await _queries.GetAll.HandleAsync(new GetAllResumeFilesQuery(), cancellationToken);
        return View(files);
    }

    // GET: /ResumeFile/Download/{id}?type=cv|cl
    [HttpGet]
    public async Task<IActionResult> Download(Guid id, string type, CancellationToken cancellationToken)
    {
        var result = await _documentService.DownloadFileAsync(id, type, cancellationToken);
        if (result is null) return NotFound("File not found.");

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    // GET: /ResumeFile/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var fileRecord = await _queries.GetById.HandleAsync(new GetResumeFileByIdQuery(id), cancellationToken);
        if (fileRecord is null) return NotFound();
        return View(fileRecord);
    }

    // POST: /ResumeFile/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> SyncSizes(CancellationToken cancellationToken)
    {
        int count = await _documentService.SyncAllFileSizesAsync(cancellationToken);
        TempData["Success"] = $"Successfully synchronized sizes for {count} files.";
        return RedirectToAction(nameof(Index));
    }
}
