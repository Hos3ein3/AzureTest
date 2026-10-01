sed -i '' '/builder.Services.ConfigureApplicationCookie/i\
    builder.Services.Configure<Microsoft.AspNetCore.Identity.SecurityStampValidatorOptions>(options =>\
    {\
        options.ValidationInterval = TimeSpan.FromHours(12);\
    });\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Program.cs
