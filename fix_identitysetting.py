path = "src/AzureTest.Domain/Documents/IdentitySetting.cs"
with open(path, "r") as f:
    content = f.read()

content = content.replace(" : BaseDocument", "")
content = content.replace("{\n    public string RedirectAfterRegister", "{\n    public Guid Id { get; init; } = Guid.CreateVersion7();\n    public string RedirectAfterRegister")

with open(path, "w") as f:
    f.write(content)
