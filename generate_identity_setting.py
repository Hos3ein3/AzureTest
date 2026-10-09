import os
import re

def ensure_dir(path):
    os.makedirs(os.path.dirname(path), exist_ok=True)

def write_file(path, content):
    ensure_dir(path)
    with open(path, "w") as f:
        f.write(content.strip())

# 1. Domain
write_file("src/AzureTest.Domain/Documents/IdentitySetting.cs", """
using System;
using System.Collections.Generic;

namespace AzureTest.Domain.Documents;

public class IdentitySetting : BaseDocument
{
    public string RedirectAfterRegister { get; set; } = "/";
    public string RedirectAfterLogin { get; set; } = "/";
    public List<string> RolesForRegister { get; set; } = new();
    public bool StayLoginAfterRegister { get; set; } = true;
}
""")

# 2. Repo Interface
write_file("src/AzureTest.Application/Interfaces/Data/NoSql/IIdentitySettingRepository.cs", """
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IIdentitySettingRepository : IDocumentRepository<IdentitySetting>
{
}
""")

# 3. Repo Impl
write_file("src/AzureTest.Persistence/Repositories/NoSql/IdentitySettingRepository.cs", """
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IdentitySettingRepository : DocumentRepository<IdentitySetting>, IIdentitySettingRepository
{
    public IdentitySettingRepository(IMongoDatabase database) : base(database, "IdentitySettings")
    {
    }
}
""")

# 4. Patch UnitOfWork Interface
uow_int_path = "src/AzureTest.Application/Interfaces/Data/NoSql/INoSqlUnitOfWork.cs"
with open(uow_int_path, "r") as f:
    uow_int_content = f.read()
if "IIdentitySettingRepository" not in uow_int_content:
    uow_int_content = uow_int_content.replace(
        "Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);",
        "IIdentitySettingRepository IdentitySettings { get; }\n    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);"
    )
    with open(uow_int_path, "w") as f:
        f.write(uow_int_content)

# 5. Patch UnitOfWork Impl
uow_impl_path = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(uow_impl_path, "r") as f:
    uow_impl_content = f.read()
if "IIdentitySettingRepository IdentitySettings" not in uow_impl_content:
    field = "private IIdentitySettingRepository? _identitySettings;"
    prop = "public IIdentitySettingRepository IdentitySettings => _identitySettings ??= new IdentitySettingRepository(_database);"
    
    # insert field
    uow_impl_content = uow_impl_content.replace(
        "private readonly IMongoDatabase _database;",
        "private readonly IMongoDatabase _database;\n    " + field
    )
    
    # insert prop
    uow_impl_content = uow_impl_content.replace(
        "public void Dispose()",
        prop + "\n\n    public void Dispose()"
    )
    with open(uow_impl_path, "w") as f:
        f.write(uow_impl_content)

# 6. Patch MongoDbClassMaps
maps_path = "src/AzureTest.Persistence/Configurations/NoSql/MongoDbClassMaps.cs"
with open(maps_path, "r") as f:
    maps_content = f.read()
if "IdentitySetting" not in maps_content:
    maps_content = maps_content.replace(
        "BsonClassMap.RegisterClassMap<IdeaSection>(cm =>",
        "BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.IdentitySetting>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n\n        BsonClassMap.RegisterClassMap<IdeaSection>(cm =>"
    )
    with open(maps_path, "w") as f:
        f.write(maps_content)

print("Domain and Repos generated successfully")
