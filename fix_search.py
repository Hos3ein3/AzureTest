import os

file = "src/AzureTest.MVC/Controllers/SearchController.cs"
with open(file, "r") as f:
    content = f.read()

# 1. Event.StartDate
content = content.replace("e.StartTime.ToString", "e.StartDate.ToString")

# 2. Resume.FindAsync
content = content.replace(
    "await _sqlUow.Resumes.GetResumesByUserIdAsync(userId, default);",
    "await _sqlUow.Resumes.FindAsync(r => r.UserId == userId);"
)

# 3. Idea -> Ideas
content = content.replace(
    "var ideas = await _noSqlUow.IdeaPages.GetAllAsync();",
    "var ideas = await _noSqlUow.Ideas.GetAllAsync();"
)

# 4. Fix implicit array typing (missing adminOnly in some anonymous objects)
content = content.replace(
    """var features = new[] {
            new { title = "Dashboard Builder", url = "/Dashboard/Manage", keywords = "dashboard, builder, home, view", icon = "fa-solid fa-table-columns" },""",
    """var features = new[] {
            new { title = "Dashboard Builder", url = "/Dashboard/Manage", keywords = "dashboard, builder, home, view", icon = "fa-solid fa-table-columns", adminOnly = false },"""
)
content = content.replace("""new { title = "Expenses", url = "/Expense", keywords = "money, cost, expense, finance", icon = "fa-solid fa-money-bill" }""", """new { title = "Expenses", url = "/Expense", keywords = "money, cost, expense, finance", icon = "fa-solid fa-money-bill", adminOnly = false }""")
content = content.replace("""new { title = "Debts", url = "/Debt", keywords = "debt, owe, lend, borrow", icon = "fa-solid fa-hand-holding-dollar" }""", """new { title = "Debts", url = "/Debt", keywords = "debt, owe, lend, borrow", icon = "fa-solid fa-hand-holding-dollar", adminOnly = false }""")
content = content.replace("""new { title = "Habits", url = "/Habit", keywords = "habit, daily, tracker", icon = "fa-solid fa-check-double" }""", """new { title = "Habits", url = "/Habit", keywords = "habit, daily, tracker", icon = "fa-solid fa-check-double", adminOnly = false }""")
content = content.replace("""new { title = "Support Tickets", url = "/Ticket", keywords = "support, ticket, help, bug", icon = "fa-solid fa-ticket" }""", """new { title = "Support Tickets", url = "/Ticket", keywords = "support, ticket, help, bug", icon = "fa-solid fa-ticket", adminOnly = false }""")
content = content.replace("""new { title = "Profile Settings", url = "/Profile", keywords = "profile, settings, user, account", icon = "fa-solid fa-user-gear" }""", """new { title = "Profile Settings", url = "/Profile", keywords = "profile, settings, user, account", icon = "fa-solid fa-user-gear", adminOnly = false }""")

with open(file, "w") as f:
    f.write(content)
print("SearchController Fixed")
