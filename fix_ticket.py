import os

file = "src/AzureTest.MVC/Controllers/TicketController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
    "ICurrentUser currentUser)",
    "ICurrentUser currentUser, IHubContext<NotificationHub> hubContext)"
)
with open(file, "w") as f:
    f.write(content)
print("TicketController fixed")
