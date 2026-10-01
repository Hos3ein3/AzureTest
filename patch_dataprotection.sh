sed -i '' '/builder.Services.AddControllersWithViews/i\
builder.Services.AddDataProtection()\
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Program.cs
