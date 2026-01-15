using System.Xml.Linq;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders XML content in a formatted code block with syntax highlighting.
/// </summary>
public record Xml : WidgetBase<Xml>
{
    public Xml(XObject xml) : this(xml.ToString() ?? string.Empty)
    {
    }

    public Xml(string content)
    {
        Content = content;
    }

    internal Xml() { }

    [Prop] public string Content { get; set; } = string.Empty;
}