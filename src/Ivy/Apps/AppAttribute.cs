
// ReSharper disable once CheckNamespace
namespace Ivy;

[AttributeUsage(AttributeTargets.Class)]
public class AppAttribute(
    string? id = null,
    string? title = null,
    Icons icon = Icons.None,
    string? description = null,
    string[]? group = null!,
    bool isVisible = true,
    int order = 0,
    bool groupExpanded = false,
    string? documentSource = null,
    string[]? searchHints = null,
    bool allowDuplicateTabs = false
)
    : Attribute
{
    public string? Id { get; set; } = id;

    public string? Title { get; set; } = title;

    public Icons? Icon { get; set; } = icon;

    public string? Description { get; set; } = description;

    public string[]? Group { get; set; } = group;

    public bool IsVisible { get; set; } = isVisible;

    public int Order { get; set; } = order;

    public bool GroupExpanded { get; set; } = groupExpanded;

    public string? DocumentSource { get; set; } = documentSource;

    public string[]? SearchHints { get; set; } = searchHints;

    public bool AllowDuplicateTabs { get; set; } = allowDuplicateTabs;
}
