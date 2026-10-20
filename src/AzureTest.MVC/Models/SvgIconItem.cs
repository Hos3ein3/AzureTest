using System;
using System.Collections.Generic;

namespace AzureTest.MVC.Models;

public sealed class SvgIconItem
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string SvgContent { get; set; } = default!;
}

public sealed class IconGalleryViewModel
{
    public IReadOnlyList<SvgIconItem> Icons { get; set; } = Array.Empty<SvgIconItem>();
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
    public string? SelectedCategory { get; set; }
    public string? SearchTerm { get; set; }
}
