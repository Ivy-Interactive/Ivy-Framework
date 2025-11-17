using System.Xml.Linq;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget for displaying XML data with syntax highlighting, collapsible nodes, and interactive navigation.</summary>
public record Xml : WidgetBase<Xml>
{
    public Xml(XObject xml) : this(xml.ToString() ?? string.Empty)
    {
    }

    public Xml(string content)
    {
        Content = content;
    }

    [Prop] public string Content { get; set; }
}