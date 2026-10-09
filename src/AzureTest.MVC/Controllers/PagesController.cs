using System.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AzureTest.Application.Pages;
using AzureTest.Contracts.DTOs.Pages;
using AzureTest.Contracts.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AzureTest.Application.Abstractions;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Page Management")]
[Description("Manage application pages and user comments.")]
public class PagesController : Controller
{
    private readonly GetAllPagesQueryHandler _getAll;
    private readonly GetPageByIdQueryHandler _getById;
    private readonly GetPageBySlugQueryHandler _getBySlug;
    private readonly CreatePageCommandHandler _create;
    private readonly UpdatePageCommandHandler _update;
    private readonly DeletePageCommandHandler _delete;
    private readonly ToggleLikeCommandHandler _toggleLike;
    private readonly ToggleDislikeCommandHandler _toggleDislike;
    private readonly AddCommentCommandHandler _addComment;
    private readonly AnswerCommentCommandHandler _answerComment;

    public PagesController(
        GetAllPagesQueryHandler getAll, GetPageByIdQueryHandler getById, GetPageBySlugQueryHandler getBySlug,
        CreatePageCommandHandler create, UpdatePageCommandHandler update, DeletePageCommandHandler delete,
        ToggleLikeCommandHandler toggleLike, ToggleDislikeCommandHandler toggleDislike,
        AddCommentCommandHandler addComment, AnswerCommentCommandHandler answerComment)
    {
        _getAll = getAll; _getById = getById; _getBySlug = getBySlug;
        _create = create; _update = update; _delete = delete;
        _toggleLike = toggleLike; _toggleDislike = toggleDislike;
        _addComment = addComment; _answerComment = answerComment;
    }

    [Authorize(Roles = "Admin")]
    [DisplayName("Index")]
    [Description("Action: Index")]
    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        var filter = new PageFilter { Search = search };
        var result = await _getAll.HandleAsync(new GetAllPagesQuery(filter));
        if (result.IsFailure) return View(new PagedResult<PageDto>(new System.Collections.Generic.List<PageDto>(), 0, 1, 10));
        
        var query = result.Value.AsQueryable();
        var total = query.Count();
        var items = query.Skip((page - 1) * 10).Take(10).ToList();
        
        ViewBag.Search = search;
        return View(new PagedResult<PageDto>(items, total, page, 10));
    }

    [Authorize(Roles = "Admin")]
    [DisplayName("Create")]
    [Description("Action: Create")]
    public IActionResult Create()
    {
        return View(new CreatePageDto());
    }
        

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [DisplayName("Create")]
    [Description("Action: Create")]
    public async Task<IActionResult> Create(CreatePageDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var cmd = new CreatePageCommand(model.Title, model.Slug, model.Content, model.ImagePath);
        var res = await _create.HandleAsync(cmd);
        if (res.IsSuccess) return RedirectToAction("Index");
        ModelState.AddModelError("", res.Error ?? "Error");
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [DisplayName("Edit")]
    [Description("Action: Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var res = await _getById.HandleAsync(new GetPageByIdQuery(id));
        if (res.IsFailure) return NotFound();
        return View(new UpdatePageDto { Id = res.Value.Id, Title = res.Value.Title, Slug = res.Value.Slug, Content = res.Value.Content, ImagePath = res.Value.ImagePath, Status = res.Value.Status });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [DisplayName("Edit")]
    [Description("Action: Edit")]
    public async Task<IActionResult> Edit(UpdatePageDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var cmd = new UpdatePageCommand(model.Id, model.Title, model.Slug, model.Content, model.ImagePath, model.Status);
        var res = await _update.HandleAsync(cmd);
        if (res.IsSuccess) return RedirectToAction("Index");
        ModelState.AddModelError("", res.Error ?? "Error");
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [DisplayName("Delete")]
    [Description("Action: Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _delete.HandleAsync(new DeletePageCommand(id));
        return RedirectToAction("Index");
    }

    // User facing page
    [AllowAnonymous]
    //[Route("Pages/{slug}")]
    [DisplayName("Read")]
    [Description("Action: Read")]
    public async Task<IActionResult> Read(string slug)
    {
        var res = await _getBySlug.HandleAsync(new GetPageBySlugQuery(slug));
        if (res.IsFailure || res.Value == null || res.Value.Status != AzureTest.Domain.Enums.PageStatus.Published) return NotFound();
        return View(res.Value);
    }

    [HttpPost]
    [DisplayName("Toggle Like")]
    [Description("Action: Toggle Like")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        await _toggleLike.HandleAsync(new ToggleLikeCommand(id, Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier))));
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    [DisplayName("Toggle Dislike")]
    [Description("Action: Toggle Dislike")]
    public async Task<IActionResult> ToggleDislike(Guid id)
    {
        await _toggleDislike.HandleAsync(new ToggleDislikeCommand(id, Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier))));
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    [DisplayName("Add Comment")]
    [Description("Action: Add Comment")]
    public async Task<IActionResult> AddComment(Guid id, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Redirect(Request.Headers["Referer"].ToString());
        await _addComment.HandleAsync(new AddCommentCommand(id, Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)), User.Identity.Name ?? "User", content));
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [DisplayName("Answer Comment")]
    [Description("Action: Answer Comment")]
    public async Task<IActionResult> AnswerComment(Guid id, Guid commentId, string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return Redirect(Request.Headers["Referer"].ToString());
        await _answerComment.HandleAsync(new AnswerCommentCommand(id, commentId, reply));
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
