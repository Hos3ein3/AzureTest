for path in ["src/AzureTest.Application/IdentitySettings/IdentitySettingCommandUnit.cs", 
             "src/AzureTest.Application/IdentitySettings/IdentitySettingQueryUnit.cs"]:
    with open(path, "r") as f:
        content = f.read()
    
    content = content.replace("public async Task<Result> ExecuteAsync", "public async Task<Result> HandleAsync")
    content = content.replace("public async Task<Result<IdentitySetting>> ExecuteAsync", "public async Task<Result<IdentitySetting>> HandleAsync")
    
    with open(path, "w") as f:
        f.write(content)
