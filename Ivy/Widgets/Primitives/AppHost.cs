using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget that hosts another Ivy application within current context (micro-frontends, plugins, modular architecture).</summary>
public record AppHost : WidgetBase<AppHost>
{
    /// <summary>Initializes app host.</summary>
    /// <param name="appId">Hosted application ID.</param>
    /// <param name="appArgs">Optional initialization arguments.</param>
    /// <param name="parentId">Optional parent application ID.</param>
    public AppHost(string appId, string? appArgs, string? parentId)
    {
        ParentId = parentId ?? string.Empty;
        AppId = appId;
        AppArgs = appArgs;
    }

    /// <summary>Hosted application ID.</summary>
    [Prop] public string AppId { get; set; }

    /// <summary>Initialization arguments for hosted app.</summary>
    [Prop] public string? AppArgs { get; set; }

    /// <summary>Parent application ID for hierarchy tracking.</summary>
    [Prop] public string? ParentId { get; set; }
}