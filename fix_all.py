import re

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

def replace_in_file(path, old, new):
    try:
        content = read_file(path)
        content = content.replace(old, new)
        write_file(path, content)
    except:
        pass

replace_in_file("test/AzureTest.UnitTest/Companies/CompanyQueriesTests.cs", "GetAllCompaniesFilters", "CompanyFilter")
replace_in_file("src/AzureTest.MVC/Controllers/CompanyController.cs", "GetAllCompaniesFilters", "CompanyFilter")

