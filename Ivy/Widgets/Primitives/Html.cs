using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Html : WidgetBase<Html>
{
    public Html(string content)
    {
        Content = content;
    }

    [Prop] public string Content { get; set; }
}