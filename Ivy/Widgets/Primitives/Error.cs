using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Error display widget with title, message, and stack trace for debugging and user feedback.</summary>
public record Error : WidgetBase<Error>
{
    /// <summary>Initializes error widget.</summary>
    /// <param name="title">Optional error title/summary.</param>
    /// <param name="message">Optional detailed error message.</param>
    /// <param name="stackTrace">Optional stack trace for debugging.</param>
    public Error(string? title = null, string? message = null, string? stackTrace = null)
    {
        Title = title;
        Message = message;
        StackTrace = stackTrace;
    }

    /// <summary>Error title/summary.</summary>
    [Prop]
    public string? Title { get; set; }

    /// <summary>Detailed error message.</summary>
    [Prop]
    public string? Message { get; set; }

    /// <summary>Stack trace for debugging.</summary>
    [Prop]
    public string? StackTrace { get; set; }
}

/// <summary>Extension methods for Error widget configuration.</summary>
public static class ErrorExtensions
{
    /// <summary>Sets error title.</summary>
    public static Error Title(this Error error, string title) => error with { Title = title };

    /// <summary>Sets error message.</summary>
    public static Error Message(this Error error, string message) => error with { Message = message };

    /// <summary>Sets stack trace.</summary>
    public static Error StackTrace(this Error error, string? stackTrace) => error with { StackTrace = stackTrace };
}