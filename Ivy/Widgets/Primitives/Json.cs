using System.Text.Json.Nodes;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for displaying JSON data with syntax highlighting and structured formatting.
/// Provides readable presentation of JSON content with proper indentation, color coding, and interactive features
/// to enhance debugging, data inspection, and API response visualization in development and production environments.
/// </summary>
/// <remarks>
/// The Json widget is designed for comprehensive JSON data presentation:
/// <list type="bullet">
/// <item><description><strong>Data visualization:</strong> Display API responses, configuration data, and structured information in readable format</description></item>
/// <item><description><strong>Development tools:</strong> Provide debugging capabilities with syntax highlighting and collapsible tree structures</description></item>
/// <item><description><strong>Interactive exploration:</strong> Enable users to explore complex JSON structures with expand/collapse functionality</description></item>
/// <item><description><strong>Copy and export:</strong> Allow users to copy formatted JSON content for external use and analysis</description></item>
/// </list>
/// <para>The widget automatically formats JSON content with proper indentation and syntax highlighting, making it ideal for displaying API responses, configuration files, and any structured data that needs to be human-readable.</para>
/// </remarks>
public record Json : WidgetBase<Json>
{
    /// <summary>
    /// Initializes a new JSON widget from a JsonNode object with automatic string conversion.
    /// Creates a formatted JSON display by converting the JsonNode to its string representation,
    /// providing a convenient way to display parsed JSON objects with proper formatting and syntax highlighting.
    /// </summary>
    /// <param name="json">The JsonNode object to display. Will be converted to formatted JSON string representation.</param>
    /// <remarks>
    /// This constructor provides seamless integration with System.Text.Json parsing:
    /// <list type="bullet">
    /// <item><description><strong>Automatic conversion:</strong> Converts JsonNode objects to formatted string representation</description></item>
    /// <item><description><strong>Type safety:</strong> Ensures valid JSON structure through JsonNode validation</description></item>
    /// <item><description><strong>Formatting preservation:</strong> Maintains proper JSON structure and formatting during conversion</description></item>
    /// <item><description><strong>Object integration:</strong> Works seamlessly with parsed JSON objects and API responses</description></item>
    /// </list>
    /// <para>Use this constructor when working with parsed JSON data from APIs, configuration files, or any scenario where you have JsonNode objects that need to be displayed to users.</para>
    /// </remarks>
    public Json(JsonNode json) : this(json.ToString())
    {
    }

    /// <summary>
    /// Initializes a new JSON widget with the specified JSON content string.
    /// Creates a formatted JSON display that renders the provided JSON string with syntax highlighting,
    /// proper indentation, and interactive features for enhanced readability and user interaction.
    /// </summary>
    /// <param name="content">The JSON content string to display. Should be valid JSON format for optimal presentation.</param>
    /// <remarks>
    /// The Json constructor provides flexible JSON content display with comprehensive formatting:
    /// <list type="bullet">
    /// <item><description><strong>Syntax highlighting:</strong> Applies color coding to JSON elements (keys, values, brackets, etc.)</description></item>
    /// <item><description><strong>Automatic formatting:</strong> Provides proper indentation and structure for readable presentation</description></item>
    /// <item><description><strong>Interactive features:</strong> Enables expand/collapse functionality for nested objects and arrays</description></item>
    /// <item><description><strong>Copy functionality:</strong> Allows users to copy formatted JSON content to clipboard</description></item>
    /// </list>
    /// <para>The widget handles both valid and malformed JSON gracefully, providing appropriate error indicators for invalid JSON while still displaying the content for debugging purposes.</para>
    /// </remarks>
    public Json(string content)
    {
        Content = content;
    }

    /// <summary>Gets or sets the JSON content string to be displayed with formatting and syntax highlighting.</summary>
    /// <value>The JSON content as a string that will be rendered with proper formatting, indentation, and syntax highlighting.</value>
    /// <remarks>
    /// The Content property holds the JSON data that will be displayed with enhanced presentation:
    /// <list type="bullet">
    /// <item><description><strong>JSON formatting:</strong> Content is automatically formatted with proper indentation and structure</description></item>
    /// <item><description><strong>Syntax highlighting:</strong> Different JSON elements (keys, strings, numbers, booleans) are color-coded for clarity</description></item>
    /// <item><description><strong>Interactive display:</strong> Large JSON structures can be collapsed and expanded for easier navigation</description></item>
    /// <item><description><strong>Validation feedback:</strong> Invalid JSON is highlighted with appropriate error indicators and messages</description></item>
    /// </list>
    /// <para>When updating this property, the widget automatically re-renders the JSON content with updated formatting and highlighting. The content can be any string, but valid JSON format provides the best user experience with full feature support.</para>
    /// </remarks>
    [Prop] public string Content { get; set; }
}