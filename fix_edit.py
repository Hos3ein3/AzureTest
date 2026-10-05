import re

file = "src/AzureTest.MVC/Controllers/ResumeController.cs"
with open(file, "r") as f:
    content = f.read()

old_str = """        await _commands.Update.HandleAsync(new UpdateResumeCommand(commandDto), cancellationToken);

        // Always update the ResumeFiles document in MongoDB (upserts if missing), replacing any attached files
        var editUploadDto = new UploadResumeFilesDto("""

new_str = """        var result = await _commands.Update.HandleAsync(new UpdateResumeCommand(commandDto), cancellationToken);
        
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
        var editUploadDto = new UploadResumeFilesDto("""

content = content.replace(old_str, new_str)
with open(file, "w") as f:
    f.write(content)
