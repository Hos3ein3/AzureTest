import os

file = "src/AzureTest.MVC/Controllers/DashboardController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
    "var cmd = new UpdateUserPreferenceCommand(userId, pref.Language, pref.ThemeColor, pref.Currency, string.Empty);",
    "var cmd = new UpdateUserPreferenceCommand(userId, pref.Language, pref.ThemeColor, pref.Currency, string.Empty, pref.IsDarkMode);"
)

content = content.replace(
    "var cmd = new UpdateUserPreferenceCommand(userId, pref?.Language ?? \"\", pref?.ThemeColor ?? \"\", pref?.Currency ?? \"USD\", slug ?? \"\");",
    "var cmd = new UpdateUserPreferenceCommand(userId, pref?.Language ?? \"\", pref?.ThemeColor ?? \"\", pref?.Currency ?? \"USD\", slug ?? \"\", pref?.IsDarkMode ?? false);"
)

with open(file, "w") as f:
    f.write(content)
print("Dashboard fixed")
