using Ivy.Shared;

namespace Ivy.Apps;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class AppAttribute(
    string? id = null,
    string? title = null,
    Icons icon = Icons.None,
    string? description = null,
    string[]? path = null!,
    bool isVisible = true,
    int order = 0,
    bool removeIvyBranding = false,
    bool groupExpanded = false,
    string? documentSource = null
)
    : System.Attribute
{
    public string? Id { get; set; } = id;

    public string? Title { get; set; } = title;

    public Icons? Icon { get; set; } = icon;

    public string? Description { get; set; } = description;

    public string[]? Path { get; set; } = path;

    public bool IsVisible { get; set; } = isVisible;

    public int Order { get; set; } = order;

    public bool RemoveIvyBranding { get; set; } = removeIvyBranding;

    public bool GroupExpanded { get; set; } = groupExpanded;

    public string? DocumentSource { get; set; } = documentSource;
}
