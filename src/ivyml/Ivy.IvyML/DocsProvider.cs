using System.Reflection;
using System.Text;

namespace Ivy.IvyML;

public static class DocsProvider
{
    public static string GetGuide()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Ivy.IvyML.Docs.IVYML.md");
        if (stream is null)
            return "Documentation not found.";
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string GetDocsOutput()
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetGuide());
        sb.AppendLine();
        sb.AppendLine("## Available Widgets");
        sb.AppendLine();
        foreach (var name in WidgetCatalog.ListNames())
            sb.AppendLine(name);
        return sb.ToString().TrimEnd();
    }

    public static string GetWidgetDocs(string widgetName)
    {
        var widget = WidgetCatalog.Get(widgetName);
        if (widget is null)
            return $"Unknown widget: {widgetName}";
        return WidgetCatalog.FormatWidgetDetail(widget);
    }
}
