def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

path = "src/AzureTest.MVC/Views/Company/Index.cshtml"
content = read_file(path)
content = content.replace("GetAllCompaniesFilters", "CompanyFilter")
write_file(path, content)
