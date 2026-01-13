using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Embed : WidgetBase<Embed>
{
    public Embed(string url)
    {
        Url = url;
    }

    internal Embed() { }

    [Prop] public string Url { get; set; } = string.Empty;
}