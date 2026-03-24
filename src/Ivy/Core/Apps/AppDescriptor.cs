using System.Diagnostics.CodeAnalysis;
using Ivy.Core.Apps;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AppIds
{
    //See GetAppOrDefault
    internal static readonly string[] ShouldNotBeAutoDefaultApps =
    [
        Auth,
        AppShell, //only default if we use app shell
        ErrorNotFound
    ];

    public const string Auth = "$auth";
    public const string AppShell = "$chrome";
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

    public required string[] Group { get; init; }

    public int Order { get; set; }

    //public string Url => "index.html?appId=" + Id;

    public Func<ViewBase>? ViewFactory { get; init; }

    public FuncViewBuilder? ViewFunc { get; init; }

    public required bool IsVisible { get; init; }

    public bool IsIndex { get; set; } = false;

    public bool IsAppShell => Id == AppIds.AppShell;

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

        if (!Type.GetConstructors().Any(c => c.GetParameters().All(p => p.HasDefaultValue)))
        {
            throw new InvalidOperationException(
                $"App '{Type.FullName}' must have a parameterless constructor. " +
                $"Use UseService<T>() inside Build() instead of constructor injection. " +
                $"Example: var client = UseService<IClientProvider>();");
        }

        return (ViewBase)Activator.CreateInstance(Type)!; // Type has DynamicallyAccessedMembers for PublicParameterlessConstructor
    }

    public MenuItem GetMenuItem()
    {
        return new MenuItem(Title, null, Icon, Id, SearchHints: SearchHints);
    }
}