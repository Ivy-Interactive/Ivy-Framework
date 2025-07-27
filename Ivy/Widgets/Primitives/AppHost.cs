using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record AppHost : WidgetBase<AppHost>
{
    public AppHost(string appId, string? appArgs, string? parentId)
    {
        ParentId = parentId ?? string.Empty;
        AppId = appId;
        AppArgs = appArgs;
    }
    [Prop] public string AppId { get; set; }
    [Prop] public string? AppArgs { get; set; }
    [Prop] public string? ParentId { get; set; }
}