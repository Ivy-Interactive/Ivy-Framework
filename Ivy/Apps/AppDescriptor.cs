using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Apps;

public static class AppIds
{
    public const string Auth = "$auth";
    public const string Chrome = "$chrome";
    public const string Default = "$default";
}

public class AppDescriptor : IAppRepositoryNode
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public Icons? Icon { get; set; }

    public string? Description { get; init; }

    public Type? Type { get; init; }

    public required string[] Path { get; init; }

    public int Order { get; set; }

    public string Url => "app.html?appId=" + Id;

    public Func<ViewBase>? ViewFactory { get; init; }

    public FuncBuilder? ViewFunc { get; init; }

    public required bool IsVisible { get; init; }

    public bool IsIndex { get; set; } = false;

    public bool IsChrome => Id == AppIds.Chrome;

    public required bool RemoveIvyBranding { get; init; }

    public bool GroupExpanded { get; set; }

    public InternalLink? Next { get; set; }
    public InternalLink? Previous { get; set; }

    public string? DocumentSource { get; set; }

    public ViewBase CreateApp()
    {
        if (ViewFactory != null)
        {
            return ViewFactory();
        }

        if (ViewFunc != null)
        {
            return new FuncView(ViewFunc);
        }

        if (Type == null)
        {
            throw new InvalidOperationException("App Type is not set.");
        }

        return (ViewBase)Activator.CreateInstance(Type)!;
    }

    public MenuItem GetMenuItem()
    {
        return new MenuItem(Title, null, Icon, Id);
    }
}