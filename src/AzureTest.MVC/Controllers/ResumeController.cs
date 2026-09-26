using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Application.Resumes.Commands.UpdateResumeStatus;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Caching.Memory;

using Microsoft.AspNetCore.Authorization;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class ResumeController : Controller
{
    private readonly IResumeCommandUnit _commands;
    private readonly IResumeQueryUnit _queries;
    private readonly IResumeValidationUnit _validators;
    private readonly IResumeDocumentService _documentService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMemoryCache _cache;

    public ResumeController(
        IResumeCommandUnit commands,
        IResumeQueryUnit queries,
        IResumeValidationUnit validators,
        IResumeDocumentService documentService,
        ICompanyRepository companyRepository,
        IMemoryCache cache)
    {
        _commands = commands;
        _queries = queries;
        _validators = validators;
        _documentService = documentService;
        _companyRepository = companyRepository;
        _cache = cache;
    }

    private async Task LoadLocationsViewBagAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "ResumeLocations";
        if (!_cache.TryGetValue(cacheKey, out List<string>? locations))
        {
            var filters = new GetAllResumesFilters(PageSize: 10000);
            var resumes = await _queries.GetAll.HandleAsync(new GetAllResumesQuery(filters), cancellationToken);
            
            locations = resumes.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Location))
                .Select(x => x.Location!)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
                
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(cacheKey, locations, cacheEntryOptions);
        }
        
        ViewBag.Locations = locations ?? new List<string>();
    }

    // Helper: loads companies into ViewBag for the form
    private async Task LoadCompaniesViewBagAsync()
    {
        var companies = await _companyRepository.GetAllAsync();
        ViewBag.Companies = companies
            .OrderBy(c => c.CompanyName)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CompanyName
            })
            .ToList();
    }

    // GET: /Resume
    [HttpGet]
    public async Task<IActionResult> Index(GetAllResumesFilters filters, CancellationToken cancellationToken)
    {
        var pagedResumes = await _queries.GetAll.HandleAsync(new GetAllResumesQuery(filters), cancellationToken);
        ViewBag.Filters = filters;
        return View(pagedResumes);
    }

    // GET: /Resume/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();

        if (resume.CompanyId.HasValue)
        {
            ViewBag.Company = await _companyRepository.GetByIdAsync(resume.CompanyId.Value);
        }

        ViewBag.StatusLogs = await _queries.GetResumeStatusLogsByResumeId.HandleAsync(new AzureTest.Application.Resumes.Queries.GetResumeStatusLogsByResumeId.GetResumeStatusLogsByResumeIdQuery(id), cancellationToken);

        ViewBag.Files = await _documentService.GetFileMetadataByResumeIdAsync(id, cancellationToken);
        return View(resume);
    }

    // GET: /Resume/Create
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadCompaniesViewBagAsync();
        await LoadLocationsViewBagAsync(cancellationToken);
        return View(new CreateResumeDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, default, DateTime.Now.Date));
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
            await LoadCompaniesViewBagAsync();
            await LoadLocationsViewBagAsync(cancellationToken);
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
        
        _cache.Remove("ResumeLocations"); // Invalidate cache so new locations appear

        TempData["Success"] = "Resume created successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Resume/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();

        ViewBag.Files = await _documentService.GetFileMetadataByResumeIdAsync(id, cancellationToken);
        await LoadCompaniesViewBagAsync();
        await LoadLocationsViewBagAsync(cancellationToken);

        var dto = new UpdateResumeDto(
            resume.Id,
            resume.PositionTitle,
            resume.Description,
            resume.Location,
            resume.JobLink,
            resume.Note,
            resume.ChatUrl,
            resume.CompanyId,
            null,
            resume.Source,
            resume.Status,
            resume.CreatedOn.ToLocalTime().Date);
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
            await LoadCompaniesViewBagAsync();
            await LoadLocationsViewBagAsync(cancellationToken);
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

        _cache.Remove("ResumeLocations"); // Invalidate cache so new locations appear

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
        await _documentService.DeleteAllFilesByResumeIdAsync(id, cancellationToken);
        TempData["Success"] = "Resume deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Resume/DownloadFile/{id}?type=cv
    [HttpGet]
    public async Task<IActionResult> DownloadFile(Guid id, string type, bool inline = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type) || (type.ToLower() != "cv" && type.ToLower() != "cl"))
            return BadRequest("Invalid file type.");

        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound("Resume not found or access denied.");

        var files = await _documentService.GetFileMetadataByResumeIdAsync(id, cancellationToken);
        if (files is null) return NotFound("File record not found.");

        var download = await _documentService.DownloadFileAsync(files.FileRecordId, type, cancellationToken);
        if (download is null) return NotFound("File not found in storage.");

        if (inline)
        {
            return File(download.Value.Content, download.Value.ContentType);
        }

        return File(download.Value.Content, download.Value.ContentType, download.Value.FileName);
    }

    // POST: /Resume/DeleteFile/{id}?type=cv
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(Guid id, string type, string returnUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(type) || (type.ToLower() != "cv" && type.ToLower() != "cl"))
            return BadRequest("Invalid file type.");

        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound("Resume not found or access denied.");

        var files = await _documentService.GetFileMetadataByResumeIdAsync(id, cancellationToken);
        if (files is not null)
        {
            await _documentService.DeleteFileAsync(files.FileRecordId, type, cancellationToken);
            TempData["Success"] = "File deleted successfully.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Resume/SearchCompanies?term=acme
    [HttpGet]
    public async Task<IActionResult> SearchCompanies(string term, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
            return Json(new List<object>());

        var results = await _companyRepository.SearchByNameAsync(term);
        var mapped = results.Select(c => new { id = c.Id, name = c.CompanyName });
        return Json(mapped);
    }

    // GET: /Resume/GetTitle/{id}
    [HttpGet]
    public async Task<IActionResult> GetTitle(Guid id, CancellationToken cancellationToken)
    {
        var resume = await _queries.GetById.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
        if (resume is null) return NotFound();
        return Json(new { id = resume.Id, title = resume.PositionTitle });
    }

    // GET: /Resume/Kanban
    [HttpGet]
    public async Task<IActionResult> Kanban(CancellationToken cancellationToken)
    {
        // Fetch all resumes with a large page size so all appear on the Kanban board
        var filters = new GetAllResumesFilters(PageSize: 1000);
        var pagedResumes = await _queries.GetAll.HandleAsync(new GetAllResumesQuery(filters), cancellationToken);
        return View(pagedResumes.Items);
    }
}
