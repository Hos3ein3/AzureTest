import re

with open('/Users/mohammadhosein/Development/AzureTest/Directory.Packages.props', 'r') as f:
    content = f.read()

if 'WebMarkupMin.AspNetCore8' not in content:
    content = content.replace('</ItemGroup>', '    <PackageVersion Include="WebMarkupMin.AspNetCore8" Version="2.17.0" />\n  </ItemGroup>')
    with open('/Users/mohammadhosein/Development/AzureTest/Directory.Packages.props', 'w') as f:
        f.write(content)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/AzureTest.MVC.csproj', 'r') as f:
    content = f.read()

if 'WebMarkupMin.AspNetCore8' not in content:
    content = content.replace('</ItemGroup>', '  <PackageReference Include="WebMarkupMin.AspNetCore8" />\n  </ItemGroup>', 1)
    with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/AzureTest.MVC.csproj', 'w') as f:
        f.write(content)
