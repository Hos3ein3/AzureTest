import re
with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'r') as f:
    content = f.read()

report_bug_method = """
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Report Bug")]
    [Description("Automatically generate a bug ticket from an error page.")]
    public async Task<IActionResult> ReportBug(string requestId, string exceptionMessage, string innerExceptionMessage)
    {
        var subject = $"Automated Bug Report - Req: {requestId}";
        var content = $"An automated bug report was generated.\\n\\n**Exception**:\\n{exceptionMessage}\\n\\n**Inner Exception**:\\n{innerExceptionMessage}";
        
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

content = re.sub(r'}\s*$', report_bug_method, content)

with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'w') as f:
    f.write(content)
