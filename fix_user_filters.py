import re

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

path = "src/AzureTest.Application/Users/Queries/GetAllUsers/GetAllUsersQueryHandler.cs"
content = read_file(path)
content = content.replace("query.SearchTerm", "query.Filters.SearchTerm")
content = content.replace("query.Role", "query.Filters.Role")
content = content.replace("query.SortBy", "query.Filters.SortBy")
content = content.replace("query.SortDescending", "query.Filters.SortDescending")
content = content.replace("query.PageNumber", "query.Filters.PageNumber")
content = content.replace("query.PageSize", "query.Filters.PageSize")
write_file(path, content)
