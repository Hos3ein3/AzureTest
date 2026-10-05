import re

file = "src/AzureTest.MVC/Controllers/ResumeController.cs"
with open(file, "r") as f:
    content = f.read()

# Add extension using
content = content.replace("using AzureTest.Application.Resumes;", "using AzureTest.Application.Resumes;\nusing AzureTest.MVC.Extensions;")

# Fix Create
old_create = """        // Convert to UTC before saving
        var commandDto = dto with { CreatedOn = dto.CreatedOn.ToUniversalTime() };
        var id = await _commands.Create.HandleAsync(new CreateResumeCommand(commandDto), cancellationToken);

        // Always create the ResumeFiles document in MongoDB, uploading any attached files
        var createUploadDto = new UploadResumeFilesDto(
            id.Value,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,
            clFile?.OpenReadStream(), clFile?.FileName, clFile?.ContentType
        );
        await _documentService.UploadFilesAsync(createUploadDto, cancellationToken);
        
        _cache.Remove("ResumeLocations"); // Invalidate cache so new locations appear

        TempData["Success"] = "Resume created successfully.";
        return RedirectToAction(nameof(Index));"""
new_create = """        // Convert to UTC before saving
        var commandDto = dto with { CreatedOn = dto.CreatedOn.ToUniversalTime() };
        var id = await _commands.Create.HandleAsync(new CreateResumeCommand(commandDto), cancellationToken);

        if (id.IsFailure)
        {
            if (id.ValidationErrors.Count > 0)
                id.AddToModelState(ModelState);
            else
                ModelState.AddModelError(string.Empty, id.Message ?? id.Error ?? "Failed to create resume.");
            
            await LoadCompaniesViewBagAsync();
            await LoadLocationsViewBagAsync(cancellationToken);
            return View(dto);
        }

        // Always create the ResumeFiles document in MongoDB, uploading any attached files
        var createUploadDto = new UploadResumeFilesDto(
            id.Value,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,
            clFile?.OpenReadStream(), clFile?.FileName, clFile?.ContentType
        );
        await _documentService.UploadFilesAsync(createUploadDto, cancellationToken);
        
        _cache.Remove("ResumeLocations"); // Invalidate cache so new locations appear

        TempData["Success"] = "Resume created successfully.";
        return RedirectToAction(nameof(Index));"""
content = content.replace(old_create, new_create)

# Fix Edit
old_edit = """        // Convert to UTC before saving
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
        return RedirectToAction(nameof(Index));"""
new_edit = """        // Convert to UTC before saving
        var commandDto = dto with { CreatedOn = dto.CreatedOn.ToUniversalTime() };
        var result = await _commands.Update.HandleAsync(new UpdateResumeCommand(commandDto), cancellationToken);
        
        if (result.IsFailure)
        {
            if (result.ValidationErrors.Count > 0)
                result.AddToModelState(ModelState);
            else
                ModelState.AddModelError(string.Empty, result.Message ?? result.Error ?? "Failed to update resume.");
            
            ViewBag.Files = await _documentService.GetFileMetadataByResumeIdAsync(id, cancellationToken);
            await LoadCompaniesViewBagAsync();
            await LoadLocationsViewBagAsync(cancellationToken);
            return View(dto);
        }

        // Always update the ResumeFiles document in MongoDB (upserts if missing), replacing any attached files
        var editUploadDto = new UploadResumeFilesDto(
            id,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,
            clFile?.OpenReadStream(), clFile?.FileName, clFile?.ContentType
        );
        await _documentService.UploadFilesAsync(editUploadDto, cancellationToken);

        _cache.Remove("ResumeLocations"); // Invalidate cache so new locations appear

        TempData["Success"] = "Resume updated successfully.";
        return RedirectToAction(nameof(Index));"""
content = content.replace(old_edit, new_edit)

# Fix UpdateStatus
old_status = """        await _commands.UpdateStatus.HandleAsync(new UpdateResumeStatusCommand(dto), cancellationToken);
        
        if (isAjax)
        {
            return Json(new { success = true, status = dto.Status.ToString() });
        }

        TempData["Success"] = "Status updated successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });"""
new_status = """        var result = await _commands.UpdateStatus.HandleAsync(new UpdateResumeStatusCommand(dto), cancellationToken);
        
        if (result.IsFailure)
        {
            if (isAjax) return Json(new { success = false, message = result.Message ?? result.Error });
            TempData["Error"] = result.Message ?? result.Error ?? "Failed to update status.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }

        if (isAjax)
        {
            return Json(new { success = true, status = dto.Status.ToString() });
        }

        TempData["Success"] = "Status updated successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });"""
content = content.replace(old_status, new_status)

# Fix Delete
old_delete = """    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        await _commands.Delete.HandleAsync(new DeleteResumeCommand(id), cancellationToken);
        await _documentService.DeleteAllFilesByResumeIdAsync(id, cancellationToken);
        TempData["Success"] = "Resume deleted successfully.";
        return RedirectToAction(nameof(Index));
    }"""
new_delete = """    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.Delete.HandleAsync(new DeleteResumeCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Message ?? result.Error ?? "Failed to delete resume.";
            return RedirectToAction(nameof(Index));
        }
        await _documentService.DeleteAllFilesByResumeIdAsync(id, cancellationToken);
        TempData["Success"] = "Resume deleted successfully.";
        return RedirectToAction(nameof(Index));
    }"""
content = content.replace(old_delete, new_delete)

with open(file, "w") as f:
    f.write(content)
