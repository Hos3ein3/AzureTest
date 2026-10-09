import re
path = "src/AzureTest.MVC/Controllers/SampleNoteController.cs"
with open(path, "r") as f: c = f.read()

c = c.replace("updated.ToViewResult(this, dto, nameof(Index));", "updated.ToFormResult(this, dto, nameof(Index));")

with open(path, "w") as f: f.write(c)
