// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The root container for an Ivy application.
/// </summary>
public record AppHost : WidgetBase<AppHost>
{
    public AppHost(string appId, string? appArgs, string? parentId)
    {
        ParentId = parentId ?? string.Empty;
        AppId = appId;
        AppArgs = appArgs;
    }

    [Prop] public string AppId { get; set; } = string.Empty;

    [Prop] public string? AppArgs { get; set; }

    [Prop] public string? ParentId { get; set; }
}