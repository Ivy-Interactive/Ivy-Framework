using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Html : WidgetBase<Html>
{
    public Html(string content)
    {
        Content = content;
    }

    internal Html() { }

    [Prop] public string Content { get; set; } = string.Empty;

    [Prop] public int Gap { get; set; } = 4;
}

public static class HtmlExtensions
{
    public static Html Gap(this Html widget, int gap) => widget with { Gap = gap };
}