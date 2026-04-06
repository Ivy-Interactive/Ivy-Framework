// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Embeds external content.
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