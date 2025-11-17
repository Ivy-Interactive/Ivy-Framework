using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core;
using Ivy.Themes;
using Ivy.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Helpers;

/// <summary>
/// Helper class for rendering user-friendly error pages with Ivy Error widgets.
/// </summary>
public static class ErrorPageHelper
{
    /// <summary>
    /// Renders an HTML error page with an Ivy Error widget embedded via meta tag.
    /// </summary>
    /// <param name="context">The HTTP context to get services and load index.html.</param>
    /// <param name="errorMessage">The error message to display.</param>
    /// <param name="statusCode">The HTTP status code (400, 401, 500, etc.).</param>
    /// <returns>An IActionResult containing the HTML error page.</returns>
    public static async Task<IActionResult> RenderErrorPage(
        HttpContext context,
        string errorMessage,
        int statusCode)
    {
        // Create exception from message
        var exception = new Exception(errorMessage);

        // Get services from HttpContext
        var serviceProvider = context.RequestServices;
        var contentBuilder = serviceProvider.GetRequiredService<IContentBuilder>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Create ErrorView
        var errorView = new ErrorView(exception);

        // Build WidgetTree
        var widgetTree = new WidgetTree(errorView, contentBuilder, serviceProvider);
        await widgetTree.BuildAsync();

        // Serialize widget to JSON
        var widgetJson = widgetTree.GetWidgets().Serialize();
        var widgetJsonString = widgetJson.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });

        // Load index.html from embedded resources
        var assembly = typeof(ErrorPageHelper).Assembly;
        var resourceName = $"{assembly.GetName().Name}.index.html";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Fallback to plaintext if index.html not found
            return new ContentResult
            {
                StatusCode = statusCode,
                ContentType = "text/plain",
                Content = errorMessage
            };
        }

        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync();

        // Inject error widget via meta tag
        // HTML encode the JSON to prevent XSS and ensure valid HTML
        var encodedJson = HtmlEncoder.Default.Encode(widgetJsonString);
        var errorWidgetMetaTag = $"<meta name=\"ivy-error-widget\" content=\"{encodedJson}\" />";

        // Insert before </head> tag
        var headEndIndex = html.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headEndIndex >= 0)
        {
            html = html.Insert(headEndIndex, $"  {errorWidgetMetaTag}\n");
        }
        else
        {
            // Fallback to replace if </head> not found
            html = html.Replace("</head>", $"  {errorWidgetMetaTag}\n</head>");
        }

        // Inject Ivy license if configured
        var ivyLicense = configuration["Ivy:License"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicense))
        {
            var encodedLicense = HtmlEncoder.Default.Encode(ivyLicense);
            var ivyLicenseTag = $"<meta name=\"ivy-license\" content=\"{encodedLicense}\" />";
            html = html.Replace("</head>", $"  {ivyLicenseTag}\n</head>");
        }

#if DEBUG
        var ivyLicensePublicKey = configuration["Ivy:LicensePublicKey"] ?? "";
        if (!string.IsNullOrEmpty(ivyLicensePublicKey))
        {
            var encodedLicensePublicKey = HtmlEncoder.Default.Encode(ivyLicensePublicKey);
            var ivyLicensePublicKeyTag =
                $"<meta name=\"ivy-license-public-key\" content=\"{encodedLicensePublicKey}\" />";
            html = html.Replace("</head>", $"  {ivyLicensePublicKeyTag}\n</head>");
        }
#endif

        // Inject theme configuration if available
        var themeService = serviceProvider.GetService<IThemeService>();
        if (themeService != null)
        {
            var themeCss = themeService.GenerateThemeCss();
            var themeMetaTag = themeService.GenerateThemeMetaTag();
            html = html.Replace("</head>", $"  {themeMetaTag}\n  {themeCss}\n</head>");
        }

        // Return HTML with proper status code
        return new ContentResult
        {
            StatusCode = statusCode,
            ContentType = "text/html; charset=utf-8",
            Content = html
        };
    }
}

