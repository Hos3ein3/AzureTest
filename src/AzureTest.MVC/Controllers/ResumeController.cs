using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Application.Resumes.Commands.UpdateResumeStatus;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AzureTest.MVC.Controllers;

public class ResumeController : Controller
{
    private readonly IResumeCommandUnit _commands;
    private readonly IResumeQueryUnit _queries;
    private readonly IResumeValidationUnit _validators;
    private readonly IResumeDocumentService _documentService;

    public ResumeController(
        IResumeCommandUnit commands,
        IResumeQueryUnit queries,
        IResumeValidationUnit validators,
        IResumeDocumentService documentService)
    {
        _commands = commands;
        _queries = queries;
        _validators = validators;
        _documentService = documentService;
    }

    // GET: /Resume
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var resumes = await _queries.GetAll.HandleAsync(new GetAllResumesQuery(), cancellationToken);
        return View(resumes);
    }

    // GET: /Resume/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();
        return View(resume);
    }

    // GET: /Resume/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateResumeDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, default, DateTime.Now.Date));
    }

    // POST: /Resume/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResumeDto dto, IFormFile? cvFile, IFormFile? clFile, CancellationToken cancellationToken)
    {
        var validation = await _validators.Create.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        // Convert to UTC before saving
        var commandDto = dto with { CreatedOn = dto.CreatedOn.ToUniversalTime() };
        var id = await _commands.Create.HandleAsync(new CreateResumeCommand(commandDto), cancellationToken);

        // Always create the ResumeFiles document in MongoDB, uploading any attached files
        var createUploadDto = new UploadResumeFilesDto(
            id,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,
            clFile?.OpenReadStream(), clFile?.FileName, clFile?.ContentType
        );
        await _documentService.UploadFilesAsync(createUploadDto, cancellationToken);

        TempData["Success"] = "Resume created successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Resume/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();

        var dto = new UpdateResumeDto(resume.Id, resume.PositionTitle, resume.Description,
            resume.Location, resume.JobLink, resume.Note, resume.Source, resume.Status, resume.CreatedOn.ToLocalTime().Date);
        return View(dto);
    }

    // POST: /Resume/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateResumeDto dto, IFormFile? cvFile, IFormFile? clFile, CancellationToken cancellationToken)
    {
        if (id != dto.Id) return BadRequest();

        var validation = await _validators.Update.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        // Convert to UTC before saving
        var commandDto = dto with { CreatedOn = dto.CreatedOn.ToUniversalTime() };
        await _commands.Update.HandleAsync(new UpdateResumeCommand(commandDto), cancellationToken);

        // Always update the ResumeFiles document in MongoDB (upserts if missing), replacing any attached files
        var editUploadDto = new UploadResumeFilesDto(
            id,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,
            clFile?.OpenReadStream(), clFile?.FileName, clFile?.ContentType
        );
        await _documentService.UploadFilesAsync(editUploadDto, cancellationToken);

        TempData["Success"] = "Resume updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Resume/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateResumeStatusDto dto, CancellationToken cancellationToken)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        var validation = await _validators.UpdateStatus.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            if (isAjax)
            {
                return Json(new { success = false, message = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)) });
            }
            TempData["Error"] = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }

        await _commands.UpdateStatus.HandleAsync(new UpdateResumeStatusCommand(dto), cancellationToken);
        
        if (isAjax)
        {
            return Json(new { success = true, status = dto.Status.ToString() });
        }

        TempData["Success"] = "Status updated successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    // GET: /Resume/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();
        return View(resume);
    }

    // POST: /Resume/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        await _commands.Delete.HandleAsync(new DeleteResumeCommand(id), cancellationToken);
        TempData["Success"] = "Resume deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
