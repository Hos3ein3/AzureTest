import re

def append_clean(path, method):
    with open(path, 'r') as f:
        content = f.read()
    
    # replace last }
    content = content.rstrip()
    if content.endswith("}"):
        content = content[:-1] + method + "\n}\n"
        
    with open(path, 'w') as f:
        f.write(content)

page_api = """
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] AzureTest.Contracts.DTOs.IdeaTracker.UpdateIdeaPageLayoutDto dto, [FromServices] AzureTest.Application.Interfaces.Messaging.ICommandHandler<AzureTest.Application.IdeaTracker.UpdateIdeaPageLayoutCommand, AzureTest.Application.Abstractions.Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new AzureTest.Application.IdeaTracker.UpdateIdeaPageLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
"""

section_api = """
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] AzureTest.Contracts.DTOs.IdeaTracker.UpdateIdeaSectionLayoutDto dto, [FromServices] AzureTest.Application.Interfaces.Messaging.ICommandHandler<AzureTest.Application.IdeaTracker.UpdateIdeaSectionLayoutCommand, AzureTest.Application.Abstractions.Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new AzureTest.Application.IdeaTracker.UpdateIdeaSectionLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
"""

append_clean('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', page_api)
append_clean('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaSectionController.cs', section_api)
