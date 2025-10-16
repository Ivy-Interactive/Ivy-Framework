---
searchHints:
  - markup
  - xml
  - data
  - format
  - structure
  - syntax
---

# Xml

<Ingress>
Display and interact with XML data in your Ivy applications with syntax highlighting, collapsible nodes, and real-time validation. Perfect for configuration files, API responses, and data feeds.
</Ingress>

The `Xml` widget displays XML data in a formatted, syntax-highlighted view. It's useful for displaying configuration files, data feeds, and other XML-structured content.

## Basic Usage

The simplest way to display XML data is by passing a string directly to the Xml widget.

```csharp demo-tabs
public class BasicXmlExample : ViewBase
{
    public override object? Build()
    {
        var simpleXml = """
            <person id="1">
                <name>John Doe</name>
                <age>30</age>
                <email>john.doe@example.com</email>
            </person>
            """;
        
        return Layout.Vertical().Gap(4)
            | new Xml(simpleXml);
    }
}
```

### Using Text Helper

You can also use the `Text.Xml()` helper method for displaying XML content inline with other text elements.

```csharp demo-tabs
public class TextHelperExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Text.P("Here's an example XML configuration:")
            | Text.Xml("<config><setting>value</setting></config>")
            | Text.P("You can also use it with state variables:")
            | Text.Xml(UseState("<root><item>dynamic</item></root>"));
    }
}
```

### Using XObject

You can also pass `XObject` instances directly to the Xml widget, which will automatically convert them to their string representation.

```csharp demo-tabs
public class XObjectXmlExample : ViewBase
{
    public override object? Build()
    {
        var xml = new System.Xml.Linq.XElement("person",
            new System.Xml.Linq.XComment("This is a comment"),
            new System.Xml.Linq.XAttribute("id", 1),
            new System.Xml.Linq.XAttribute("source", "web"),
            new System.Xml.Linq.XElement("name", "John Doe"),
            new System.Xml.Linq.XElement("age", 30),
            new System.Xml.Linq.XElement("isStudent", false),
            new System.Xml.Linq.XElement("address",
                new System.Xml.Linq.XElement("street", "123 Main St"),
                new System.Xml.Linq.XElement("city", "Anytown"),
                new System.Xml.Linq.XElement("state", "NY"),
                new System.Xml.Linq.XElement("zip", "12345")
            ),
            new System.Xml.Linq.XElement("phoneNumbers",
                new System.Xml.Linq.XElement("phoneNumber", "555-1234"),
                new System.Xml.Linq.XElement("phoneNumber", "555-5678")
            )
        );
        
        return Layout.Vertical().Gap(4)
            | new Xml(xml);
    }
}
```

<WidgetDocs Type="Ivy.Xml" ExtensionTypes="Ivy.XmlExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Xml.cs"/>

## Examples

<Details>
<Summary>
Interactive XML Editor
</Summary>
<Body>
This example shows how to create an interactive XML editor with real-time preview.

```csharp demo-tabs
public class InteractiveXmlEditor : ViewBase
{
    public override object? Build()
    {
        var xmlContent = this.UseState("""
            <person>
                <name>Jane Doe</name>
                <age>25</age>
                <skills>
                    <skill>C#</skill>
                    <skill>JavaScript</skill>
                    <skill>Python</skill>
                </skills>
            </person>
            """);
        
        var isValid = this.UseState(true);
        var errorMessage = this.UseState("");
        
        void ValidateXml()
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xmlContent.Value);
                isValid.Value = true;
                errorMessage.Value = "";
            }
            catch (Exception ex)
            {
                isValid.Value = false;
                errorMessage.Value = ex.Message;
            }
        }
        
        // Validate on content change
        UseEffect(() => {
            ValidateXml();
        });
        
        return Layout.Vertical().Gap(4)
            | Text.Label("XML Editor")
            | xmlContent.ToTextAreaInput(placeholder: "Enter XML content here...")
                .Height(Size.Units(50))
            | (isValid.Value 
                ? new Xml(xmlContent.Value)
                : Text.Danger($"Invalid XML: {errorMessage.Value}"))
            | new Button("Validate XML", onClick: _ => ValidateXml());
    }
}
```

</Body>
</Details>
