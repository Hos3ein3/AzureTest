path = "src/AzureTest.MVC/Services/PermissionScanner.cs"
with open(path, "r") as f: c = f.read()

# Replace the skip logic
old_logic = """
                // Skip if DisplayName or Description is missing (inner API)
                if (actionDisplayNameAttr == null || actionDescAttr == null)
                {
                    continue;
                }

                var aDisplayName = actionDisplayNameAttr.DisplayName;
                var aDesc = actionDescAttr.Description;
"""
new_logic = """
                var aDisplayName = actionDisplayNameAttr?.DisplayName ?? actionName;
                var aDesc = actionDescAttr?.Description ?? $"{controllerName} {actionName}";
"""
c = c.replace(old_logic, new_logic)
with open(path, "w") as f: f.write(c)
print("Scanner fixed")
