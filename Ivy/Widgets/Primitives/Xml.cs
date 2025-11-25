using System.Xml.Linq;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

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