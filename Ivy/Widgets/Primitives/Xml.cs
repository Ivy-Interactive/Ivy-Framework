using System.Xml.Linq;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget for displaying XML data with syntax highlighting, collapsible nodes, and interactive navigation.</summary>
public record Xml : WidgetBase<Xml>
{
    /// <summary>Initializes XML widget from XObject with automatic string conversion.</summary>
    public Xml(XObject xml) : this(xml.ToString() ?? string.Empty)
    {
    }

    /// <summary>Initializes XML widget with specified XML content string.</summary>
    public Xml(string content)
    {
        Content = content;
    }

    /// <summary>XML content to display with syntax highlighting and collapsible tree structure.</summary>
    [Prop] public string Content { get; set; }
}