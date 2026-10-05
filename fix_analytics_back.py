import re

file = "src/AzureTest.MVC/Controllers/Api/AnalyticsEndpointController.cs"
with open(file, "r") as f:
    content = f.read()

old_str = """        [HttpGet("resume-analytic/{id}")]
        public async Task<IActionResult> GetResumeInfo(Guid id, [FromQuery] string? viewName, CancellationToken cancellationToken)
        {
            var data = await _getAnalyticsQuery.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
            if (data.IsFailure)
            {
                return NotFound(data.Message ?? data.Error ?? "Resume not found.");
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                return PartialView($"~/Views/Shared/Widgets/{viewName}.cshtml", data.Value);
            }

            return Ok(data.Value);
        }"""
new_str = """        [HttpGet("resume-analytic/{id}")]
        public async Task<IActionResult> GetResumeInfo(Guid id, [FromQuery] string? viewName, CancellationToken cancellationToken)
        {
            var data = await _getAnalyticsQuery.HandleAsync(new GetResumeByIdQuery(id), cancellationToken);
            if (data == null)
            {
                return NotFound("Resume not found.");
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                return PartialView($"~/Views/Shared/Widgets/{viewName}.cshtml", data);
            }

            return Ok(data);
        }"""

content = content.replace(old_str, new_str)
with open(file, "w") as f:
    f.write(content)
