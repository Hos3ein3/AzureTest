import re

file = "src/AzureTest.MVC/Controllers/ResumeController.cs"
with open(file, "r") as f:
    content = f.read()

old_str = """        var id = await _commands.Create.HandleAsync(new CreateResumeCommand(commandDto), cancellationToken);

        // Always create the ResumeFiles document in MongoDB, uploading any attached files
        var createUploadDto = new UploadResumeFilesDto(
            id.Value,"""

new_str = """        var id = await _commands.Create.HandleAsync(new CreateResumeCommand(commandDto), cancellationToken);

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
            id.Value,"""

content = content.replace(old_str, new_str)
with open(file, "w") as f:
    f.write(content)
