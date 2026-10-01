import re

def insert_before_last_brace(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    # Find the last closing brace and insert return Result.Success(); before it
    # We can just match the method signature, then find its closing brace.
    # But since these are short files with usually one method that needs it:
    
    # Let's do it by regex searching for specific method endings if they don't have return
    pass

import sys

def patch_file(filepath, before, after):
    with open(filepath, 'r') as f:
        content = f.read()
    if before in content:
        content = content.replace(before, after)
        with open(filepath, 'w') as f:
            f.write(content)
            
patch_file("src/AzureTest.Application/Resumes/Commands/UpdateResumeStatus/UpdateResumeStatusCommandHandler.cs", 
"UnitOfWork.Resumes.Update(resume);\n    }\n}", 
"UnitOfWork.Resumes.Update(resume);\n        return Result.Success();\n    }\n}")

patch_file("src/AzureTest.Application/Dashboards/Commands/CreateDashboardComponentCommand.cs",
"await _unitOfWork.DashboardComponents.AddAsync(component, cancellationToken);\n    }\n}",
"await _unitOfWork.DashboardComponents.AddAsync(component, cancellationToken);\n        return Result.Success();\n    }\n}")

patch_file("src/AzureTest.Application/Dashboards/Commands/CreateUserDashboardCommand.cs",
"await _unitOfWork.UserDashboards.AddAsync(dashboard, cancellationToken);\n    }\n}",
"await _unitOfWork.UserDashboards.AddAsync(dashboard, cancellationToken);\n        return Result.Success();\n    }\n}")

patch_file("src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs",
"await _unitOfWork.DashboardComponents.AddAsync(component, cancellationToken);\n    }",
"await _unitOfWork.DashboardComponents.AddAsync(component, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs",
"await _unitOfWork.DashboardComponents.UpdateAsync(component.Id, component, cancellationToken);\n    }",
"await _unitOfWork.DashboardComponents.UpdateAsync(component.Id, component, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs",
"if (component == null) return;",
"if (component == null) return Result.Failure(\"Not found\");")

patch_file("src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs",
"await _unitOfWork.DashboardComponents.RemoveAsync(command.Id, cancellationToken);\n    }",
"await _unitOfWork.DashboardComponents.RemoveAsync(command.Id, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs",
"public async Task<Result<IReadOnlyList<DashboardRawComponent>>> HandleAsync(GetAllRawComponentsQuery query, CancellationToken cancellationToken = default)",
"public async Task<IReadOnlyList<DashboardRawComponent>> HandleAsync(GetAllRawComponentsQuery query, CancellationToken cancellationToken = default)")

patch_file("src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs",
"await _unitOfWork.DashboardRawComponents.AddAsync(component, cancellationToken);\n    }",
"await _unitOfWork.DashboardRawComponents.AddAsync(component, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs",
"await _unitOfWork.DashboardRawComponents.RemoveAsync(command.Id, cancellationToken);\n    }",
"await _unitOfWork.DashboardRawComponents.RemoveAsync(command.Id, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs",
"if (component == null) return;",
"if (component == null) return Result.Failure(\"Not found\");")

patch_file("src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs",
"await _unitOfWork.DashboardRawComponents.UpdateAsync(component.Id, component, cancellationToken);\n    }",
"await _unitOfWork.DashboardRawComponents.UpdateAsync(component.Id, component, cancellationToken);\n        return Result.Success();\n    }")

patch_file("src/AzureTest.Application/UserPreferences/Commands/UpdateUserPreferenceCommand.cs",
"await _unitOfWork.UserPreferences.UpdateAsync(preference.Id, preference, cancellationToken);\n        }\n    }\n}",
"await _unitOfWork.UserPreferences.UpdateAsync(preference.Id, preference, cancellationToken);\n        }\n        return Result.Success();\n    }\n}")

