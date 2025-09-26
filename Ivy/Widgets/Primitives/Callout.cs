using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual and semantic variants available for callout widgets.
/// Each variant provides distinct styling and conveys different types of information
/// to users through color schemes, icons, and visual emphasis.
/// </summary>
public enum CalloutVariant
{
    /// <summary>Informational callout for general information, tips, or neutral messages.</summary>
    Info,
    /// <summary>Warning callout for cautionary information that requires user attention.</summary>
    Warning,
    /// <summary>Error callout for critical issues, failures, or problems that need immediate attention.</summary>
    Error,
    /// <summary>Success callout for positive feedback, confirmations, or successful operations.</summary>
    Success
}

/// <summary>
/// A prominent message widget designed to draw user attention to important information.
/// Provides visually distinct presentation of messages with contextual styling, icons, and semantic meaning
/// through different variants for information, warnings, errors, and success notifications.
/// </summary>
public record Callout : WidgetBase<Callout>
{
    /// <summary>
    /// Initializes a new callout with the specified content and styling options.
    /// Creates a visually prominent message container with automatic content processing
    /// and variant-appropriate styling for effective user communication.
    /// </summary>
    /// <param name="description">The main content of the callout. Can be a string (converted to Markdown) or any widget object.</param>
    /// <param name="title">Optional title displayed prominently at the top of the callout.</param>
    /// <param name="variant">The visual and semantic variant determining the callout's appearance and meaning. Default is Info.</param>
    /// <param name="icon">Optional icon displayed alongside the title for visual reinforcement of the message type.</param>
    /// <remarks>
    /// The Callout widget is designed for prominent user communication:
    /// <list type="bullet">
    /// <item><description><strong>Information display:</strong> Present important information that users should notice</description></item>
    /// <item><description><strong>Status communication:</strong> Communicate success, warning, or error states</description></item>
    /// <item><description><strong>User guidance:</strong> Provide tips, instructions, or contextual help</description></item>
    /// <item><description><strong>Attention drawing:</strong> Highlight critical information that requires user awareness</description></item>
    /// </list>
    /// <para>String descriptions are automatically converted to Markdown for rich text formatting support.</para>
    /// </remarks>
    public Callout(object? description = null, string? title = null, CalloutVariant variant = CalloutVariant.Info, Icons? icon = null)
    {
        var child = description switch
        {
            string str => new Markdown(str),
            _ => description
        };

        if (child != null)
            Children = [child!];

        Title = title;
        Variant = variant;
        Icon = icon;
    }

    /// <summary>Gets or sets the title displayed prominently at the top of the callout.</summary>
    /// <value>The title text, or null if no title should be displayed.</value>
    /// <remarks>
    /// The title provides a concise summary of the callout's message and is displayed with emphasis
    /// to quickly communicate the main point to users scanning the interface.
    /// </remarks>
    [Prop] public string? Title { get; set; }

    /// <summary>Gets or sets the visual and semantic variant of the callout.</summary>
    /// <value>The CalloutVariant determining the callout's appearance and semantic meaning.</value>
    /// <remarks>
    /// The variant controls the color scheme, default icon, and visual emphasis of the callout,
    /// helping users quickly understand the type and importance of the message being communicated.
    /// </remarks>
    [Prop] public CalloutVariant Variant { get; set; }

    /// <summary>Gets or sets the icon displayed alongside the title for visual reinforcement.</summary>
    /// <value>The icon from the Icons enumeration, or null if no icon should be displayed.</value>
    /// <remarks>
    /// Icons provide immediate visual context for the callout's purpose and help users
    /// quickly identify the type of message without reading the text content.
    /// </remarks>
    [Prop] public Icons? Icon { get; set; }

    /// <summary>
    /// Creates an informational callout for general information, tips, or neutral messages.
    /// </summary>
    /// <param name="description">The main content of the callout.</param>
    /// <param name="title">Optional title for the callout.</param>
    /// <returns>A new callout with Info variant styling.</returns>
    public static Callout Info(string? description = null, string? title = null) => new(description, title);

    /// <summary>
    /// Creates a warning callout for cautionary information that requires user attention.
    /// </summary>
    /// <param name="description">The main content of the callout.</param>
    /// <param name="title">Optional title for the callout.</param>
    /// <returns>A new callout with Warning variant styling.</returns>
    public static Callout Warning(string? description = null, string? title = null) => new(description, title, CalloutVariant.Warning);

    /// <summary>
    /// Creates an error callout for critical issues, failures, or problems that need immediate attention.
    /// </summary>
    /// <param name="description">The main content of the callout.</param>
    /// <param name="title">Optional title for the callout.</param>
    /// <returns>A new callout with Error variant styling.</returns>
    public static Callout Error(string? description = null, string? title = null) => new(description, title, CalloutVariant.Error);

    /// <summary>
    /// Creates a success callout for positive feedback, confirmations, or successful operations.
    /// </summary>
    /// <param name="description">The main content of the callout.</param>
    /// <param name="title">Optional title for the callout.</param>
    /// <returns>A new callout with Success variant styling.</returns>
    public static Callout Success(string? description = null, string? title = null) => new(description, title, CalloutVariant.Success);
}

/// <summary>
/// Provides extension methods for configuring callout widgets with fluent syntax.
/// Enables convenient configuration of callout properties including title, description, variant, and icon
/// through method chaining for improved readability and ease of use in callout customization.
/// </summary>
public static class CalloutExtensions
{
    /// <summary>
    /// Sets the title for the callout.
    /// </summary>
    /// <param name="callout">The callout to configure.</param>
    /// <param name="title">The title text to display prominently at the top of the callout.</param>
    /// <returns>The callout with the specified title.</returns>
    /// <remarks>
    /// The title provides a concise summary of the callout's message and helps users
    /// quickly understand the main point without reading the full description.
    /// </remarks>
    public static Callout Title(this Callout callout, string title)
    {
        return callout with { Title = title };
    }

    /// <summary>
    /// Sets the description content for the callout using Markdown formatting.
    /// </summary>
    /// <param name="callout">The callout to configure.</param>
    /// <param name="description">The description text in Markdown format to display as the main content.</param>
    /// <returns>The callout with the specified description content.</returns>
    /// <remarks>
    /// The description is automatically converted to a Markdown widget, enabling rich text formatting
    /// including bold, italic, links, lists, and other Markdown features for enhanced content presentation.
    /// </remarks>
    public static Callout Description(this Callout callout, string description)
    {
        return callout with { Children = [new Markdown(description)] };
    }

    /// <summary>
    /// Sets the visual and semantic variant of the callout.
    /// </summary>
    /// <param name="callout">The callout to configure.</param>
    /// <param name="variant">The CalloutVariant determining the callout's appearance and semantic meaning.</param>
    /// <returns>The callout with the specified variant styling.</returns>
    /// <remarks>
    /// The variant controls the color scheme, visual emphasis, and semantic meaning of the callout,
    /// helping users immediately understand the type and importance of the message.
    /// </remarks>
    public static Callout Variant(this Callout callout, CalloutVariant variant)
    {
        return callout with { Variant = variant };
    }

    /// <summary>
    /// Sets the icon displayed alongside the title for visual reinforcement.
    /// </summary>
    /// <param name="callout">The callout to configure.</param>
    /// <param name="icon">The icon from the Icons enumeration to display with the callout.</param>
    /// <returns>The callout with the specified icon.</returns>
    /// <remarks>
    /// Icons provide immediate visual context and help users quickly identify the callout's purpose
    /// and message type through recognizable visual symbols.
    /// </remarks>
    public static Callout Icon(this Callout callout, Icons icon)
    {
        return callout with { Icon = icon };
    }
}