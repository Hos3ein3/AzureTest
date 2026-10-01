import re
import os

files_to_fix = [
    "src/AzureTest.Application/Dashboards/Commands/CreateDashboardComponentCommand.cs",
    "src/AzureTest.Application/Dashboards/Commands/UpdateUserDashboardCommand.cs",
    "src/AzureTest.Application/Dashboards/Commands/CreateUserDashboardCommand.cs",
    "src/AzureTest.Application/DebtPersons/Commands/DebtPersonCommands.cs",
    "src/AzureTest.Application/Dashboards/Commands/DeleteUserDashboardCommand.cs",
    "src/AzureTest.Application/ExpenseCategories/Commands/ExpenseCategoryCommands.cs",
    "src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs",
    "src/AzureTest.Application/UserPreferences/Commands/UpdateUserPreferenceCommand.cs",
]

for filepath in files_to_fix:
    if not os.path.exists(filepath): continue
    with open(filepath, 'r') as f:
        content = f.read()

    # Find async Task<Result> methods that don't return
    # We can just look for "await _unitOfWork..." or similar followed by "}" and append "return Result.Success();"
    # Actually, simpler: replace `    }\n}` with `        return Result.Success();\n    }\n}` if it's a handler.
    # We can do this safely by looking at methods:
    
    methods = re.findall(r'(public async Task<Result> HandleAsync[^{]+{.*?)(?=\n    })', content, re.DOTALL)
    for m in methods:
        if 'return' not in m.split('\n')[-1] and 'throw' not in m.split('\n')[-1]:
            content = content.replace(m + '\n    }', m + '\n        return Result.Success();\n    }')
            
    with open(filepath, 'w') as f:
        f.write(content)

# For CreateHabitCommandHandler.cs: Cannot implicitly convert type Task<Guid> to Task<Result<Guid>>
# Let's fix that too
habit = "src/AzureTest.Application/Habits/Commands/CreateHabitCommandHandler.cs"
with open(habit, 'r') as f:
    hc = f.read()
hc = hc.replace("return habit.Id;", "return Result.Success(habit.Id);")
with open(habit, 'w') as f:
    f.write(hc)

# For LogoutUserCommand.cs
logout = "src/AzureTest.Application/Users/Commands/LogoutUser/LogoutUserCommand.cs"
with open(logout, 'r') as f:
    lc = f.read()
lc = lc.replace("return true;", "return Result.Success(true);")
with open(logout, 'w') as f:
    f.write(lc)
    
