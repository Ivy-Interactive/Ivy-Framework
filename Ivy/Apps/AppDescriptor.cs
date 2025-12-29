using System.Diagnostics.CodeAnalysis;
using Ivy.Shared;
using Ivy.Views;

namespace Ivy.Apps;

public static class AppIds
{
    //See GetAppOrDefault
    internal static readonly string[] ShouldNotBeAutoDefaultApps =
    [
        Auth,
        Chrome, //only default if we use chrome 
        ErrorNotFound
    ];

    public const string Auth = "$auth";
    public const string Chrome = "$chrome";
    public const string Default = "$default";
    public const string ErrorNotFound = "$error-not-found";
}

public class AppDescriptor : IAppRepositoryNode
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public Icons? Icon { get; set; }

    public string? Description { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? Type { get; init; }

    public required string[] Path { get; init; }

    public int Order { get; set; }

    //public string Url => "index.html?appId=" + Id;

    public Func<ViewBase>? ViewFactory { get; init; }

    public FuncViewBuilder? ViewFunc { get; init; }

    public required bool IsVisible { get; init; }

    public bool IsIndex { get; set; } = false;

    public bool IsChrome => Id == AppIds.Chrome;

    public bool GroupExpanded { get; set; }

    public InternalLink? Next { get; set; }

    public InternalLink? Previous { get; set; }

    public string? DocumentSource { get; set; }

    public string[]? SearchHints { get; set; }

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

        return (ViewBase)Activator.CreateInstance(Type)!; // Type has DynamicallyAccessedMembers for PublicParameterlessConstructor
    }

    public MenuItem GetMenuItem()
    {
        return new MenuItem(Title, null, Icon, Id, SearchHints: SearchHints);
    }
}