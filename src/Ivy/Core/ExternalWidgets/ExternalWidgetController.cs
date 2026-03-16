using Microsoft.AspNetCore.Mvc;

namespace Ivy.Core.ExternalWidgets;

/// <summary>
/// Controller that serves external widget assets (JS/CSS) from embedded resources.
/// </summary>
[ApiController]
[Route("ivy/external-widgets")]
public class ExternalWidgetController : ControllerBase
{
    /// <summary>
    /// Gets the external widget registry for the frontend.
    /// </summary>
    [HttpGet("registry")]
    public IActionResult GetRegistry()
    {
        var registry = ExternalWidgetRegistry.Instance.GetRegistryForFrontend();
        return Ok(registry);
    }

    /// <summary>
    /// Serves the JavaScript module for an external widget.
    /// </summary>
    [HttpGet("{typeName}/script.js")]
    public IActionResult GetScript(string typeName)
    {
        var decodedTypeName = Uri.UnescapeDataString(typeName);
        var widgetInfo = ExternalWidgetRegistry.Instance.GetWidget(decodedTypeName);

        if (widgetInfo == null)
        {
            return NotFound($"External widget '{decodedTypeName}' not found");
        }

        var stream = GetEmbeddedResource(widgetInfo.Assembly, widgetInfo.ResourceBasePath, widgetInfo.ScriptPath);
        if (stream == null)
        {
            return NotFound($"Script resource not found for widget '{decodedTypeName}'");
        }

        if (Ivy.ProcessHelper.IsDevelopment())
        {
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        }
        else
        {
            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
        return File(stream, "application/javascript");
    }

    /// <summary>
    /// Serves the CSS file for an external widget (if it has one).
    /// </summary>
    [HttpGet("{typeName}/style.css")]
    public IActionResult GetStyle(string typeName)
    {
        var decodedTypeName = Uri.UnescapeDataString(typeName);
        var widgetInfo = ExternalWidgetRegistry.Instance.GetWidget(decodedTypeName);

        if (widgetInfo == null)
        {
            return NotFound($"External widget '{decodedTypeName}' not found");
        }

        if (widgetInfo.StylePath == null)
        {
            return NotFound($"Widget '{decodedTypeName}' does not have a separate stylesheet");
        }

        var stream = GetEmbeddedResource(widgetInfo.Assembly, widgetInfo.ResourceBasePath, widgetInfo.StylePath);
        if (stream == null)
        {
            return NotFound($"Style resource not found for widget '{decodedTypeName}'");
        }

        if (Ivy.ProcessHelper.IsDevelopment())
        {
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        }
        else
        {
            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
        return File(stream, "text/css");
    }

    private static Stream? GetEmbeddedResource(System.Reflection.Assembly assembly, string basePath, string relativePath)
    {
        // Convert path separators to dots for embedded resource naming
        var resourcePath = relativePath.Replace('/', '.').Replace('\\', '.');

        // Try different naming conventions
        var candidates = new[]
        {
            $"{basePath}.{resourcePath}",
            $"{basePath.Replace('-', '_')}.{resourcePath}",
            $"{assembly.GetName().Name}.{resourcePath}",
            resourcePath
        };

        foreach (var candidate in candidates)
        {
            var stream = assembly.GetManifestResourceStream(candidate);
            if (stream != null)
            {
                return stream;
            }
        }

        return null;
    }
}
