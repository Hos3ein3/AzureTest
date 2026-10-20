using System;
using System.Collections.Generic;

namespace AzureTest.MVC.Models;

public class BreadcrumbItem
{
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }

    public BreadcrumbItem() { }

    public BreadcrumbItem(string Label, string? Url = null, string? Icon = null, bool IsActive = false, bool active = false)
    {
        this.Label = Label;
        this.Url = Url;
        this.Icon = Icon;
        this.IsActive = IsActive || active;
    }
}

public record HeaderAction(string Text, string? Url = null, string? Icon = "fa-solid fa-plus", string? Variant = null);

public class PageHeaderModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Icon { get; set; } = "fa-solid fa-shapes";
    public string? Badge { get; set; }
    public IReadOnlyList<BreadcrumbItem>? Breadcrumbs { get; set; }
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionIcon { get; set; } = "fa-solid fa-plus";

    public PageHeaderModel() { }

    public PageHeaderModel(
        string Title,
        string? Subtitle = null,
        string? Icon = "fa-solid fa-shapes",
        string? Badge = null,
        IReadOnlyList<BreadcrumbItem>? Breadcrumbs = null,
        string? ActionText = null,
        string? ActionUrl = null,
        string? ActionIcon = "fa-solid fa-plus",
        IReadOnlyList<BreadcrumbItem>? breadcrumbs = null)
    {
        this.Title = Title;
        this.Subtitle = Subtitle;
        this.Icon = Icon;
        this.Badge = Badge;
        this.Breadcrumbs = breadcrumbs ?? Breadcrumbs;
        this.ActionText = ActionText;
        this.ActionUrl = ActionUrl;
        this.ActionIcon = ActionIcon;
    }

    public PageHeaderModel(
        string Title,
        string? Subtitle,
        string? Icon,
        HeaderAction? Action)
    {
        this.Title = Title;
        this.Subtitle = Subtitle;
        this.Icon = Icon;
        if (Action != null)
        {
            this.ActionText = Action.Text;
            this.ActionUrl = Action.Url;
            this.ActionIcon = Action.Icon;
        }
    }
}

public class EmptyStateModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-solid fa-inbox";
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionIcon { get; set; } = "fa-solid fa-plus";

    public EmptyStateModel() { }

    public EmptyStateModel(
        string Title,
        string Description,
        string Icon = "fa-solid fa-inbox",
        string? ActionText = null,
        string? ActionUrl = null,
        string? ActionIcon = "fa-solid fa-plus")
    {
        this.Title = Title;
        this.Description = Description;
        this.Icon = Icon;
        this.ActionText = ActionText;
        this.ActionUrl = ActionUrl;
        this.ActionIcon = ActionIcon;
    }

    public EmptyStateModel(
        string Title,
        string Description,
        string Icon,
        HeaderAction? Action)
    {
        this.Title = Title;
        this.Description = Description;
        this.Icon = Icon;
        if (Action != null)
        {
            this.ActionText = Action.Text;
            this.ActionUrl = Action.Url;
            this.ActionIcon = Action.Icon;
        }
    }
}

public class PaginationModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public long TotalItems { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string BaseUrl { get; set; } = string.Empty;
    public IDictionary<string, string>? RouteParams { get; set; }
    public Func<int, string>? PageUrlFactory { get; set; }

    public PaginationModel() { }

    public PaginationModel(
        int CurrentPage,
        int TotalPages,
        long TotalItems,
        int PageSize,
        string BaseUrl = "",
        IDictionary<string, string>? RouteParams = null,
        Func<int, string>? PageUrlFactory = null)
    {
        this.CurrentPage = CurrentPage;
        this.TotalPages = TotalPages;
        this.TotalItems = TotalItems;
        this.PageSize = PageSize;
        this.BaseUrl = BaseUrl;
        this.RouteParams = RouteParams;
        this.PageUrlFactory = PageUrlFactory;
    }

    public PaginationModel(
        int CurrentPage,
        int TotalPages,
        int PageSize,
        long TotalItems,
        Func<int, string>? PageUrlFactory = null)
    {
        this.CurrentPage = CurrentPage;
        this.TotalPages = TotalPages;
        this.PageSize = PageSize;
        this.TotalItems = TotalItems;
        this.PageUrlFactory = PageUrlFactory;
    }
}
