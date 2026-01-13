using System.Collections.Concurrent;
using System.Reflection;

namespace Ivy.Core.ExternalWidgets;

/// <summary>
/// Information about a registered external widget.
/// </summary>
public record ExternalWidgetInfo
{
    /// <summary>
    /// The full type name used in serialization (e.g., "MyCompany.Widgets.SuperChart").
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// The path to the JavaScript module file.
    /// </summary>
    public required string ScriptPath { get; init; }

    /// <summary>
    /// Optional path to a CSS file.
    /// </summary>
    public string? StylePath { get; init; }

    /// <summary>
    /// The name of the exported React component.
    /// </summary>
    public required string ExportName { get; init; }

    /// <summary>
    /// The name of the global variable created by the IIFE bundle.
    /// </summary>
    public string? GlobalName { get; init; }

    /// <summary>
    /// The assembly containing the widget and its assets.
    /// </summary>
    public required Assembly Assembly { get; init; }

    /// <summary>
    /// The base resource path in the assembly for embedded resources.
    /// </summary>
    public required string ResourceBasePath { get; init; }
}

/// <summary>
/// DTO sent to the frontend containing external widget registration info.
/// </summary>
public record ExternalWidgetRegistryDto
{
    public required string TypeName { get; init; }
    public required string ScriptUrl { get; init; }
    public string? StyleUrl { get; init; }
    public required string ExportName { get; init; }

    /// <summary>
    /// The name of the global variable created by the IIFE bundle.
    /// </summary>
    public string? GlobalName { get; init; }
}

/// <summary>
/// Registry that discovers and manages external widgets from loaded assemblies.
/// </summary>
public class ExternalWidgetRegistry
{
    private readonly ConcurrentDictionary<string, ExternalWidgetInfo> _widgets = new();
    private bool _isInitialized;
    private readonly object _initLock = new();

    /// <summary>
    /// Singleton instance of the registry.
    /// </summary>
    public static ExternalWidgetRegistry Instance { get; } = new();

    private ExternalWidgetRegistry() { }

    /// <summary>
    /// Initializes the registry by scanning all loaded assemblies for external widgets.
    /// Safe to call multiple times - will only scan once.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;

        lock (_initLock)
        {
            if (_isInitialized) return;

            ScanAssemblies();
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Manually register an assembly to scan for external widgets.
    /// Useful for dynamically loaded assemblies.
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        ScanAssembly(assembly);
    }

    /// <summary>
    /// Gets all registered external widgets.
    /// </summary>
    public IReadOnlyDictionary<string, ExternalWidgetInfo> GetAll() => _widgets;

    /// <summary>
    /// Gets information about a specific external widget by type name.
    /// </summary>
    public ExternalWidgetInfo? GetWidget(string typeName)
    {
        return _widgets.GetValueOrDefault(typeName);
    }

    /// <summary>
    /// Checks if a widget type is an external widget.
    /// </summary>
    public bool IsExternalWidget(string typeName)
    {
        return _widgets.ContainsKey(typeName);
    }

    /// <summary>
    /// Gets the registry data formatted for sending to the frontend.
    /// </summary>
    public IEnumerable<ExternalWidgetRegistryDto> GetRegistryForFrontend()
    {
        return _widgets.Values.Select(w =>
        {
            var version = w.Assembly.GetName().Version?.ToString() ?? "0";
            return new ExternalWidgetRegistryDto
            {
                TypeName = w.TypeName,
                ScriptUrl = $"/ivy/external-widgets/{Uri.EscapeDataString(w.TypeName)}/script.js?v={version}",
                StyleUrl = w.StylePath != null
                    ? $"/ivy/external-widgets/{Uri.EscapeDataString(w.TypeName)}/style.css?v={version}"
                    : null,
                ExportName = w.ExportName,
                GlobalName = w.GlobalName
            };
        });
    }

    private void ScanAssemblies()
    {
        var scannedAssemblies = new HashSet<string>();

        // First scan already-loaded assemblies
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !IsSystemAssembly(a))
            .ToList();

        foreach (var assembly in loadedAssemblies)
        {
            ScanAssemblySafe(assembly, scannedAssemblies);
        }

        // Scan all DLLs in the application base directory
        // This catches external widget assemblies that haven't been loaded yet
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        foreach (var dllPath in Directory.GetFiles(baseDir, "*.dll"))
        {
            var fileName = Path.GetFileNameWithoutExtension(dllPath);

            // Skip system/third-party assemblies
            if (IsSystemAssemblyName(fileName))
                continue;

            // Skip already scanned
            if (scannedAssemblies.Any(s => s.Contains(fileName)))
                continue;

            try
            {
                var assembly = Assembly.LoadFrom(dllPath);

                // Only scan assemblies that reference Ivy (external widgets must reference Ivy)
                if (!ReferencesIvy(assembly))
                    continue;

                ScanAssemblySafe(assembly, scannedAssemblies);
            }
            catch
            {
                // Silently skip assemblies that can't be loaded
            }
        }
    }

    private void ScanAssemblySafe(Assembly assembly, HashSet<string> scannedAssemblies)
    {
        var fullName = assembly.FullName ?? assembly.GetName().Name ?? "";
        if (scannedAssemblies.Contains(fullName))
            return;

        scannedAssemblies.Add(fullName);

        try
        {
            ScanAssembly(assembly);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExternalWidgetRegistry] Failed to scan assembly {fullName}: {ex.Message}");
        }
    }

    private static bool IsSystemAssemblyName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        return name.StartsWith("System") ||
               name.StartsWith("Microsoft") ||
               name.StartsWith("netstandard") ||
               name.StartsWith("mscorlib") ||
               name.StartsWith("Newtonsoft") ||
               name.StartsWith("NuGet") ||
               name.StartsWith("Grpc") ||
               name.StartsWith("Google") ||
               name.StartsWith("Antlr") ||
               name.StartsWith("Azure") ||
               name.StartsWith("Humanizer") ||
               name.StartsWith("SixLabors") ||
               name.StartsWith("SkiaSharp") ||
               name.StartsWith("Bogus") ||
               name.StartsWith("OpenAI") ||
               name.StartsWith("Apache") ||
               name.StartsWith("DeepCloner") ||
               name.StartsWith("ExcelNumberFormat") ||
               name.StartsWith("Isopoh") ||
               name.StartsWith("YamlDotNet") ||
               name.StartsWith("Serilog") ||
               name.StartsWith("Castle") ||
               name.StartsWith("Moq") ||
               name.StartsWith("xunit") ||
               name.StartsWith("FluentAssertions") ||
               name.StartsWith("NSubstitute") ||
               name.StartsWith("Polly") ||
               name.StartsWith("AutoMapper") ||
               name.StartsWith("Dapper") ||
               name.StartsWith("MediatR") ||
               name.StartsWith("FluentValidation") ||
               name.StartsWith("Swashbuckle") ||
               name.StartsWith("NLog") ||
               name.StartsWith("log4net") ||
               name.StartsWith("StackExchange") ||
               name.StartsWith("Npgsql") ||
               name.StartsWith("MySql") ||
               name.StartsWith("Oracle") ||
               name.StartsWith("MongoDB") ||
               name.StartsWith("RabbitMQ") ||
               name.StartsWith("MassTransit") ||
               name.StartsWith("Quartz") ||
               name.StartsWith("Hangfire") ||
               name.StartsWith("ClosedXML") ||
               name.StartsWith("EPPlus") ||
               name.StartsWith("NPOI") ||
               name.StartsWith("CsvHelper") ||
               name.StartsWith("HtmlAgilityPack") ||
               name.StartsWith("AngleSharp") ||
               name.StartsWith("Markdig") ||
               name.StartsWith("MimeKit") ||
               name.StartsWith("MailKit") ||
               name.StartsWith("ICSharpCode") ||
               name.StartsWith("DotNetZip") ||
               name.StartsWith("SharpZipLib");
    }

    /// <summary>
    /// Checks if an assembly references Ivy (required for external widgets).
    /// </summary>
    private static bool ReferencesIvy(Assembly assembly)
    {
        try
        {
            return assembly.GetReferencedAssemblies()
                .Any(r => r.Name?.Equals("Ivy", StringComparison.OrdinalIgnoreCase) == true);
        }
        catch
        {
            return false;
        }
    }

    private void ScanAssembly(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Some types may fail to load, use the ones that succeeded
            types = ex.Types.Where(t => t != null).ToArray()!;
        }

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<ExternalWidgetAttribute>();
            if (attr == null) continue;

            // Verify the type implements IWidget
            if (!typeof(IWidget).IsAssignableFrom(type))
            {
                Console.WriteLine($"[ExternalWidgetRegistry] Warning: {type.FullName} has [ExternalWidget] but does not implement IWidget");
                continue;
            }

            var typeName = WidgetSerializer.CleanTypeName(type);
            var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? "Unknown";

            // Verify the embedded resource exists
            var resourceBasePath = assemblyName.Replace('-', '_').Replace('.', '_');
            var scriptResourceName = $"{resourceBasePath}.{attr.ScriptPath.Replace('/', '.')}";

            if (assembly.GetManifestResourceInfo(scriptResourceName) == null)
            {
                // Try alternative naming conventions
                var altScriptResourceName = $"{assemblyName}.{attr.ScriptPath.Replace('/', '.')}";
                if (assembly.GetManifestResourceInfo(altScriptResourceName) == null)
                {
                    Console.WriteLine($"[ExternalWidgetRegistry] Warning: Script resource '{scriptResourceName}' not found for {typeName}");
                    Console.WriteLine($"[ExternalWidgetRegistry] Available resources in {assemblyName}:");
                    foreach (var name in assembly.GetManifestResourceNames())
                    {
                        Console.WriteLine($"  - {name}");
                    }
                }
            }

            // Auto-derive GlobalName from namespace if not explicitly specified
            // e.g., "Ivy.Widgets.Tiptap" -> "Ivy_Widgets_Tiptap"
            var globalName = attr.GlobalName ?? type.Namespace?.Replace('.', '_') ?? type.Name;

            var info = new ExternalWidgetInfo
            {
                TypeName = typeName,
                ScriptPath = attr.ScriptPath,
                StylePath = attr.StylePath,
                ExportName = attr.ExportName,
                GlobalName = globalName,
                Assembly = assembly,
                ResourceBasePath = assemblyName
            };

            if (_widgets.TryAdd(typeName, info))
            {
                //Console.WriteLine($"[ExternalWidgetRegistry] Registered external widget: {typeName}");
            }
        }
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.FullName ?? "";
        return name.StartsWith("System.") ||
               name.StartsWith("Microsoft.") ||
               name.StartsWith("netstandard") ||
               name.StartsWith("mscorlib");
    }
}
