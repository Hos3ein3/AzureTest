import os

# Create Hub
os.makedirs("src/AzureTest.MVC/Hubs", exist_ok=True)
with open("src/AzureTest.MVC/Hubs/NotificationHub.cs", "w") as f:
    f.write("""using Microsoft.AspNetCore.SignalR;
namespace AzureTest.MVC.Hubs;

public class NotificationHub : Hub
{
}
""")

# Program.cs
prog_file = "src/AzureTest.MVC/Program.cs"
with open(prog_file, "r") as f:
    prog_content = f.read()

if "AddSignalR()" not in prog_content:
    prog_content = prog_content.replace(
        "builder.Services.AddControllersWithViews()",
        "builder.Services.AddControllersWithViews()\n    .AddRazorRuntimeCompilation();\nbuilder.Services.AddSignalR();"
    )

if "MapHub<" not in prog_content:
    prog_content = prog_content.replace(
        "app.MapControllerRoute(",
        "app.MapHub<AzureTest.MVC.Hubs.NotificationHub>(\"/notificationHub\");\n\napp.MapControllerRoute("
    )
with open(prog_file, "w") as f:
    f.write(prog_content)

# TicketController.cs
tick_file = "src/AzureTest.MVC/Controllers/TicketController.cs"
with open(tick_file, "r") as f:
    tick_content = f.read()

if "IHubContext<" not in tick_content:
    tick_content = tick_content.replace(
        "using Microsoft.AspNetCore.Mvc;",
        "using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.SignalR;\nusing AzureTest.MVC.Hubs;"
    )
    tick_content = tick_content.replace(
        "private readonly ICurrentUser _currentUser;",
        "private readonly ICurrentUser _currentUser;\n    private readonly IHubContext<NotificationHub> _hubContext;"
    )
    tick_content = tick_content.replace(
        "ITicketCommandUnit ticketCommand)",
        "ITicketCommandUnit ticketCommand, IHubContext<NotificationHub> hubContext)"
    )
    tick_content = tick_content.replace(
        "_ticketCommand = ticketCommand;",
        "_ticketCommand = ticketCommand;\n        _hubContext = hubContext;"
    )
    tick_content = tick_content.replace(
        "return RedirectToAction(nameof(Details), new { id = ticketId.Value });",
        "await _hubContext.Clients.All.SendAsync(\"ReceiveTicketNotification\", \"New Ticket\", subject);\n            return RedirectToAction(nameof(Details), new { id = ticketId.Value });"
    )
    # Also in AutoReport:
    tick_content = tick_content.replace(
        "return Ok();",
        "await _hubContext.Clients.All.SendAsync(\"ReceiveTicketNotification\", \"Auto Report\", subject);\n            return Ok();"
    )
    with open(tick_file, "w") as f:
        f.write(tick_content)

print("SignalR Patched")
