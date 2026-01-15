using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Embeds generic external content. Can act as a container for various media types.
/// </summary>
public record Embed : WidgetBase<Embed>
{
    public Embed(string url)
    {
        Url = url;
    }

    internal Embed() { }

    [Prop] public string Url { get; set; } = string.Empty;
}