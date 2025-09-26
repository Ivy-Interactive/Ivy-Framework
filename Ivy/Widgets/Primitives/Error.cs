using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A specialized widget for displaying error information with structured presentation of error details.
/// Provides comprehensive error display capabilities including title, message, and stack trace information
/// for debugging, user feedback, and error reporting scenarios with appropriate visual styling and organization.
/// </summary>
public record Error : WidgetBase<Error>
{
    /// <summary>
    /// Initializes a new error display widget with optional error details.
    /// Creates a structured error presentation that can display various levels of error information
    /// from simple user messages to detailed debugging information including stack traces.
    /// </summary>
    /// <param name="title">Optional error title or summary providing a concise description of the error type.</param>
    /// <param name="message">Optional detailed error message explaining what went wrong and potential solutions.</param>
    /// <param name="stackTrace">Optional technical stack trace information for debugging purposes.</param>
    /// <remarks>
    /// The Error widget is designed for comprehensive error communication:
    /// <list type="bullet">
    /// <item><description><strong>User-facing errors:</strong> Display friendly error messages with helpful information</description></item>
    /// <item><description><strong>Development debugging:</strong> Show detailed error information including stack traces</description></item>
    /// <item><description><strong>Exception handling:</strong> Present caught exceptions in a structured, readable format</description></item>
    /// <item><description><strong>Error reporting:</strong> Collect and display error information for analysis and support</description></item>
    /// </list>
    /// <para>The widget supports progressive disclosure, showing basic information to users while providing detailed technical information when needed.</para>
    /// </remarks>
    public Error(string? title = null, string? message = null, string? stackTrace = null)
    {
        Title = title;
        Message = message;
        StackTrace = stackTrace;
    }

    /// <summary>Gets or sets the error title or summary providing a concise description of the error.</summary>
    /// <value>The error title text, or null if no title is specified.</value>
    /// <remarks>
    /// The title serves as a brief, user-friendly summary of the error that helps users quickly understand
    /// the nature of the problem. It should be concise and descriptive without technical jargon.
    /// </remarks>
    [Prop] public string? Title { get; set; }

    /// <summary>Gets or sets the detailed error message explaining what went wrong and potential solutions.</summary>
    /// <value>The error message text, or null if no message is specified.</value>
    /// <remarks>
    /// The message provides detailed information about the error, including what caused it and
    /// potential steps for resolution. It should be helpful for both users and developers.
    /// </remarks>
    [Prop] public string? Message { get; set; }

    /// <summary>Gets or sets the technical stack trace information for debugging purposes.</summary>
    /// <value>The stack trace text, or null if no stack trace is available.</value>
    /// <remarks>
    /// The stack trace provides detailed technical information about where the error occurred in the code.
    /// This information is primarily useful for developers and should be displayed in a way that doesn't
    /// overwhelm end users, such as in a collapsible section or separate debug view.
    /// </remarks>
    [Prop] public string? StackTrace { get; set; }
}

/// <summary>
/// Provides extension methods for configuring error display widgets with fluent syntax.
/// Enables convenient configuration of error properties including title, message, and stack trace
/// through method chaining for improved readability and ease of use in error handling scenarios.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Sets the error title or summary for the error display.
    /// </summary>
    /// <param name="error">The error widget to configure.</param>
    /// <param name="title">The error title providing a concise description of the error type.</param>
    /// <returns>The error widget with the specified title.</returns>
    /// <remarks>
    /// The title should be brief and user-friendly, helping users quickly understand the nature
    /// of the error without overwhelming them with technical details.
    /// </remarks>
    public static Error Title(this Error error, string title) => error with { Title = title };

    /// <summary>
    /// Sets the detailed error message explaining what went wrong and potential solutions.
    /// </summary>
    /// <param name="error">The error widget to configure.</param>
    /// <param name="message">The detailed error message with explanation and potential solutions.</param>
    /// <returns>The error widget with the specified message.</returns>
    /// <remarks>
    /// The message should provide helpful information about the error cause and suggest
    /// actionable steps for resolution when possible.
    /// </remarks>
    public static Error Message(this Error error, string message) => error with { Message = message };

    /// <summary>
    /// Sets the technical stack trace information for debugging purposes.
    /// </summary>
    /// <param name="error">The error widget to configure.</param>
    /// <param name="stackTrace">The stack trace information, or null to remove existing stack trace.</param>
    /// <returns>The error widget with the specified stack trace information.</returns>
    /// <remarks>
    /// Stack trace information is primarily useful for developers and debugging scenarios.
    /// Consider displaying this information in a collapsible or separate section to avoid overwhelming users.
    /// </remarks>
    public static Error StackTrace(this Error error, string? stackTrace) => error with { StackTrace = stackTrace };
}