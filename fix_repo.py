import re

path = "src/AzureTest.Application/Pages/PageCommandUnit.cs"
with open(path, "r") as f: c = f.read()
c = c.replace("UpdateAsync(doc)", "UpdateAsync(doc.Id, doc)")
c = c.replace("DeleteAsync(cmd.Id)", "RemoveAsync(cmd.Id)")
with open(path, "w") as f: f.write(c)

path2 = "src/AzureTest.Application/Pages/PageQueryUnit.cs"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace("var all = await _noSql.Pages.GetAllAsync();", "IEnumerable<PageDocument> all = await _noSql.Pages.GetAllAsync();")
with open(path2, "w") as f: f.write(c2)

print("Fixed repo calls")
