using System.Reflection;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Test.Widgets.External;

/// <summary>
/// Guards the contract between the eight built-in external widgets and the resources embedded in
/// Ivy.dll by <c>Ivy.csproj</c>'s <c>EmbedExternalWidgetResources</c> target. A typo in either the
/// <c>[ExternalWidget]</c> path or the MSBuild <c>LogicalName</c> only surfaces at runtime as a
/// 404, so it is asserted here instead.
/// </summary>
public class ExternalWidgetResourceTests
{
    private static readonly Assembly IvyAssembly = typeof(ExternalWidgetRegistry).Assembly;

    /// <summary>
    /// Both <c>TiptapInput</c> and <c>TiptapInput&lt;TString&gt;</c> carry [ExternalWidget], but
    /// CleanTypeName strips the generic arity so they share a single registration.
    /// </summary>
    private static readonly string[] ExpectedTypeNames =
    [
        "Ivy.Widgets.ActivityHeatmap.ActivityHeatmap",
        "Ivy.Widgets.AnimatedStatusLabel.AnimatedStatusLabel",
        "Ivy.Widgets.DiffView.DiffView",
        "Ivy.Widgets.Leaflet.Map",
        "Ivy.Widgets.QRCode.QRCode",
        "Ivy.Widgets.ScreenshotFeedback.ScreenshotFeedback",
        "Ivy.Widgets.Tiptap.TiptapInput",
        "Ivy.Widgets.Xterm.Terminal",
    ];

    public static TheoryData<string> BuiltInTypeNames()
    {
        var data = new TheoryData<string>();
        foreach (var typeName in ExpectedTypeNames)
        {
            data.Add(typeName);
        }
        return data;
    }

    [Fact]
    public void AllBuiltInWidgetsAreRegistered()
    {
        var registered = BuiltInWidgets()
            .Select(w => w.TypeName)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedTypeNames.OrderBy(n => n, StringComparer.Ordinal).ToArray(), registered);
    }

    [Theory]
    [MemberData(nameof(BuiltInTypeNames))]
    public void ScriptResourceResolvesToNonEmptyStream(string typeName)
    {
        var widget = GetWidget(typeName);

        using var stream = OpenResource(widget, widget.ScriptPath);
        Assert.True(stream.Length > 0, $"Script resource for '{typeName}' is empty");
    }

    [Theory]
    [MemberData(nameof(BuiltInTypeNames))]
    public void StyleResourceResolvesToNonEmptyStreamWhenDeclared(string typeName)
    {
        var widget = GetWidget(typeName);
        if (widget.StylePath == null) return;

        using var stream = OpenResource(widget, widget.StylePath);
        Assert.True(stream.Length > 0, $"Style resource for '{typeName}' is empty");
    }

    [Theory]
    [MemberData(nameof(BuiltInTypeNames))]
    public void GlobalNameMatchesNamespaceWithUnderscores(string typeName)
    {
        var widget = GetWidget(typeName);
        var widgetNamespace = typeName[..typeName.LastIndexOf('.')];

        Assert.Equal(widgetNamespace.Replace('.', '_'), widget.GlobalName);
    }

    private static IEnumerable<ExternalWidgetInfo> BuiltInWidgets()
    {
        ExternalWidgetRegistry.Instance.RegisterAssembly(IvyAssembly);
        return ExternalWidgetRegistry.Instance.GetAll().Values.Where(w => w.Assembly == IvyAssembly);
    }

    private static ExternalWidgetInfo GetWidget(string typeName)
    {
        var widget = BuiltInWidgets().SingleOrDefault(w => w.TypeName == typeName);
        Assert.NotNull(widget);
        return widget;
    }

    /// <summary>
    /// Mirrors the primary lookup performed by <c>ExternalWidgetController</c>: the embedded resource
    /// name is the assembly name followed by the dotted relative path.
    /// </summary>
    private static Stream OpenResource(ExternalWidgetInfo widget, string relativePath)
    {
        var resourceName = $"{widget.ResourceBasePath}.{relativePath.Replace('/', '.')}";
        var stream = widget.Assembly.GetManifestResourceStream(resourceName);

        Assert.NotNull(stream);
        return stream;
    }
}
