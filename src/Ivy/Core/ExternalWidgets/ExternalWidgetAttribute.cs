namespace Ivy;

/// <summary>
/// Marks a widget as an external widget with frontend assets bundled in the assembly.
/// </summary>
/// <param name="scriptPath">
/// The path to the JavaScript module file relative to the assembly's embedded resources.
/// Example: "frontend/dist/SuperChartWidget.js"
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ExternalWidgetAttribute(string scriptPath) : Attribute
{
    /// <summary>
    /// The path to the JavaScript module file.
    /// </summary>
    public string ScriptPath { get; } = scriptPath;

    /// <summary>
    /// Optional path to a CSS file. If not specified, CSS should be bundled in the JS.
    /// </summary>
    public string? StylePath { get; set; }

    /// <summary>
    /// The name of the exported React component in the module.
    /// Defaults to "default" (the default export).
    /// </summary>
    public string ExportName { get; set; } = "default";

    /// <summary>
    /// The name of the global variable created by the IIFE bundle.
    /// This is the 'name' property in the vite.config lib options.
    /// Required for IIFE bundles that contain multiple widgets.
    /// </summary>
    public string? GlobalName { get; set; }
}
