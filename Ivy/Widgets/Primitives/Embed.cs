using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Embed widget for external content (videos, maps, docs, etc). Security: iframe sandboxing applied.</summary>
public record Embed : WidgetBase<Embed>
{
    /// <summary>Initializes embed widget.</summary>
    /// <param name="url">External content URL.</param>
    public Embed(string url)
    {
        Url = url;
    }

    /// <summary>External content URL.</summary>
    [Prop] public string Url { get; set; }
}