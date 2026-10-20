using System.IO;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace AzureTest.UnitTest.Food;

public class ViewResolutionTests
{
    private static string GetMvcDirectory()
    {
        var solutionDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(solutionDir) && !File.Exists(Path.Combine(solutionDir, "AzureTest.sln")))
        {
            var parent = Directory.GetParent(solutionDir);
            if (parent == null) break;
            solutionDir = parent.FullName;
        }
        return Path.Combine(solutionDir, "src", "AzureTest.MVC");
    }

    [Fact]
    public void PhysicalFileProvider_ShouldFind_IconIndexView()
    {
        var mvcDir = GetMvcDirectory();
        var provider = new PhysicalFileProvider(mvcDir);

        var fileInfo = provider.GetFileInfo("/Views/Icon/Index.cshtml");
        fileInfo.Exists.Should().BeTrue($"File should exist at {fileInfo.PhysicalPath}");

        var fileInfoRelative = provider.GetFileInfo("Views/Icon/Index.cshtml");
        fileInfoRelative.Exists.Should().BeTrue();
    }

    [Fact]
    public void PhysicalFileProvider_ShouldFind_UiComponentsView()
    {
        var mvcDir = GetMvcDirectory();
        var provider = new PhysicalFileProvider(mvcDir);

        var fileInfo = provider.GetFileInfo("/Views/UiComponents/Index.cshtml");
        fileInfo.Exists.Should().BeTrue($"File should exist at {fileInfo.PhysicalPath}");

        var fileInfoRelative = provider.GetFileInfo("Views/UiComponents/Index.cshtml");
        fileInfoRelative.Exists.Should().BeTrue();
    }
}
