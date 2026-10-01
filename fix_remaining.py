def replace_in_file(path, old, new):
    try:
        with open(path, 'r') as f:
            c = f.read()
        if old in c:
            c = c.replace(old, new)
            with open(path, 'w') as f:
                f.write(c)
    except:
        pass

replace_in_file("src/AzureTest.MVC/Views/Company/Index.cshtml", "CompanyFilter", "GetAllCompaniesFilters")

replace_in_file("test/AzureTest.UnitTest/Companies/CompanyQueriesTests.cs", "CompanyFilter", "GetAllCompaniesFilters")
replace_in_file("test/AzureTest.UnitTest/Companies/CompanyCommandsTests.cs", "result.Should().NotBeEmpty();", "result.Value.Should().NotBeEmpty();")
replace_in_file("test/AzureTest.UnitTest/Companies/CompanyCommandsTests.cs", "result.Should().BeTrue();", "result.IsSuccess.Should().BeTrue();")

replace_in_file("src/AzureTest.MVC/Controllers/AuthController.cs", "ModelState.AddModelError('', result.Error);", "ModelState.AddModelError(\"\", result.Error);")
replace_in_file("src/AzureTest.MVC/Controllers/AuthController.cs", "if (!result.IsSuccess)", "if (result.IsFailure)")
replace_in_file("src/AzureTest.MVC/Controllers/AuthController.cs", "result.Error", "result.Value.Errors[0]") # For register user tuple? Wait let's just do it manually in a sec if it fails.

replace_in_file("src/AzureTest.MVC/Controllers/ResumeController.cs", "return RedirectToAction(nameof(Details), new { id = result });", "return RedirectToAction(nameof(Details), new { id = result.Value });")
replace_in_file("src/AzureTest.MVC/Controllers/ResumeController.cs", "return RedirectToAction(nameof(Details), new { id = result.Id });", "return RedirectToAction(nameof(Details), new { id = result.Value });")

