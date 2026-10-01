import re

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

path = "src/AzureTest.Application/Companies/Queries/GetAllCompanies/GetAllCompaniesQuery.cs"
content = read_file(path)
content = content.replace("GetAllCompaniesFilters", "CompanyFilter")
write_file(path, content)
