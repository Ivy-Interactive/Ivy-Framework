using System.Xml.Linq;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.CodeXml, path: ["Widgets", "Primitives"])]
public class XmlApp : SampleBase
{
    protected override object? BuildSample()
    {
        var xml = new XElement("person",
            new XComment("This is a comment"),
            new XAttribute("id", 1),
            new XAttribute("source", "web"),
            new XElement("name", "John Doe"),
            new XElement("age", 30),
            new XElement("isStudent", false),
            new XElement("address",
                new XElement("street", "123 Main St"),
                new XElement("city", "Anytown"),
                new XElement("state", "NY"),
                new XElement("zip", "12345")
            ),
            new XElement("phoneNumbers",
                new XElement("phoneNumber", "555-1234"),
                new XElement("phoneNumber", "555-5678")
            )
        );
        return xml;
    }
}