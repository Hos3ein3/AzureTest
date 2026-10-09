path2 = "src/AzureTest.Application/Pages/PageQueryUnit.cs"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace("IEnumerable<PageDocument> all = await _noSql.Pages.GetAllAsync();", "IEnumerable<PageDocument> all = (await _noSql.Pages.GetAllAsync()).AsEnumerable();")
with open(path2, "w") as f: f.write(c2)
