import re
with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'r') as f:
    content = f.read()

replacement = """
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Report Bug")]
    [Description("Automatically generate a bug ticket from an error page.")]
    public async Task<IActionResult> ReportBug(string requestId, string exceptionMessage, string innerExceptionMessage)
    {
        var subject = $"Automated Bug Report - Req: {requestId}";
        var content = $"An automated bug report was generated.\\r\\n\\r\\nException:\\r\\n{exceptionMessage}\\r\\n\\r\\nInner Exception:\\r\\n{innerExceptionMessage}";
        
        var ticketId = await _ticketCommand.CreateTicket.HandleAsync(new CreateTicketCommand(subject, content, TicketPriority.High), default);
        
        if (ticketId.IsSuccess)
        {
            TempData["SuccessMessage"] = "Bug report submitted successfully! Thank you.";
            return RedirectToAction(nameof(Details), new { id = ticketId.Value });
        }
        
        return RedirectToAction("Index", "Home");
    }
}
"""

content = re.sub(r'\[HttpPost\]\s*\[ValidateAntiForgeryToken\]\s*\[DisplayName\("Report Bug"\)\].*?}\s*}', replacement, content, flags=re.DOTALL)

with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'w') as f:
    f.write(content)
