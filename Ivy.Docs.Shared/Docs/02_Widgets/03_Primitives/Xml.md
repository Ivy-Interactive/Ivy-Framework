# Xml

The `Xml` widget displays XML data in a formatted, syntax-highlighted view. It's useful for displaying configuration files, data feeds, and other XML-structured content.

```csharp demo-tabs 
public class XmlConfigView : ViewBase
{
    public override object? Build()
    {
        var sampleXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <configuration>
              <appSettings>
                <add key="environment" value="production" />
                <add key="logLevel" value="info" />
                <add key="maxConnections" value="100" />
              </appSettings>
              <connectionStrings>
                <add name="mainDb" 
                     connectionString="Server=db.example.com;Database=maindb;User Id=admin;Password=****;" />
                <add name="reportingDb" 
                     connectionString="Server=reports.example.com;Database=reports;User Id=reporter;Password=****;" />
              </connectionStrings>
              <security>
                <authentication mode="Forms">
                  <forms loginUrl="~/login" timeout="30" />
                </authentication>
                <authorization>
                  <deny users="?" />
                  <allow roles="admin,editor" />
                </authorization>
              </security>
            </configuration>
            """;
            
        return Layout.Vertical()
            | new Xml(sampleXml);
    }
}
```

<WidgetDocs Type="Ivy.Xml" ExtensionTypes="Ivy.XmlExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Xml.cs"/>
