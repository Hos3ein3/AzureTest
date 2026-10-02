import re

def append_to_controller(path, code):
    with open(path, 'r') as f:
        content = f.read()
    
    content = content.replace("}", code + "\n}")
    
    with open(path, 'w') as f:
        f.write(content)

page_api = """
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] UpdateIdeaPageLayoutDto dto, [FromServices] ICommandHandler<UpdateIdeaPageLayoutCommand, Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateIdeaPageLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
"""

section_api = """
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] UpdateIdeaSectionLayoutDto dto, [FromServices] ICommandHandler<UpdateIdeaSectionLayoutCommand, Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateIdeaSectionLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
"""

append_to_controller('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', page_api)
append_to_controller('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaSectionController.cs', section_api)
