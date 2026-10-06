import os

# 1. Update Domain
file = "src/AzureTest.Domain/Documents/UserPreference.cs"
with open(file, "r") as f:
    content = f.read()
if "bool IsDarkMode" not in content:
    content = content.replace(
        "public string PreferredDashboard { get; set; } = string.Empty;",
        "public string PreferredDashboard { get; set; } = string.Empty;\n    public bool IsDarkMode { get; set; } = false;"
    )
    with open(file, "w") as f:
        f.write(content)

# 2. Update Command
file = "src/AzureTest.Application/UserPreferences/Commands/UpdateUserPreferenceCommand.cs"
with open(file, "r") as f:
    content = f.read()
if "bool IsDarkMode" not in content:
    content = content.replace(
        "string Currency, string PreferredDashboard",
        "string Currency, string PreferredDashboard, bool IsDarkMode"
    )
    content = content.replace(
        "pref.PreferredDashboard = request.PreferredDashboard;",
        "pref.PreferredDashboard = request.PreferredDashboard;\n        pref.IsDarkMode = request.IsDarkMode;"
    )
    with open(file, "w") as f:
        f.write(content)

# 3. Update Controller
file = "src/AzureTest.MVC/Controllers/UserPreferenceController.cs"
with open(file, "r") as f:
    content = f.read()

if "IsDarkMode" not in content:
    content = content.replace(
        "preferredDashboard = pref.PreferredDashboard ?? string.Empty,",
        "preferredDashboard = pref.PreferredDashboard ?? string.Empty,\n            isDarkMode = pref.IsDarkMode,"
    )
    content = content.replace(
        "public string? PreferredDashboard { get; set; }",
        "public string? PreferredDashboard { get; set; }\n        public bool? IsDarkMode { get; set; }"
    )
    content = content.replace(
        "string? preferredDashboard = jsonRequest?.PreferredDashboard;",
        "string? preferredDashboard = jsonRequest?.PreferredDashboard;\n        bool? isDarkMode = jsonRequest?.IsDarkMode;"
    )
    content = content.replace(
        "var finalPreferredDashboard = preferredDashboard ?? string.Empty;",
        "var finalPreferredDashboard = preferredDashboard ?? string.Empty;\n        \n        var pref = await _getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(_currentUser.Id), cancellationToken);\n        var finalIsDarkMode = isDarkMode ?? pref.IsDarkMode;"
    )
    content = content.replace(
        "new UpdateUserPreferenceCommand(_currentUser.Id, language, themeColor, finalCurrency, finalPreferredDashboard),",
        "new UpdateUserPreferenceCommand(_currentUser.Id, language, themeColor, finalCurrency, finalPreferredDashboard, finalIsDarkMode),"
    )
    content = content.replace(
        "preferredDashboard = finalPreferredDashboard });",
        "preferredDashboard = finalPreferredDashboard, isDarkMode = finalIsDarkMode });"
    )
    
    with open(file, "w") as f:
        f.write(content)
print("Patched Prefs")
