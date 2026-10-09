for path, old_base, new_base in [
    ("src/AzureTest.Application/IdentitySettings/IdentitySettingCommandUnit.cs", 
     "BaseCommandHandler<UpdateIdentitySettingCommand, Result>", "ICommandHandler<UpdateIdentitySettingCommand, Result>"),
    ("src/AzureTest.Application/IdentitySettings/IdentitySettingQueryUnit.cs",
     "BaseQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>", "IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>")
]:
    with open(path, "r") as f:
        content = f.read()
    
    content = content.replace(old_base, new_base)
    content = content.replace("protected override async Task<Result", "public async Task<Result")
    
    with open(path, "w") as f:
        f.write(content)
print("Interfaces fixed")
