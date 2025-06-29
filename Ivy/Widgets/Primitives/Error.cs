using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Error : WidgetBase<Error>
{
    public Error(string? title = null, string? message = null, string? stackTrace = null)
    {
        Title = title;
        Message = message;
        StackTrace = stackTrace;
    }

    [Prop] public string? Title { get; set; }
    [Prop] public string? Message { get; set; }
    [Prop] public string? StackTrace { get; set; }
}

public static class ErrorExtensions
{
    public static Error Title(this Error error, string title) => error with { Title = title };
    public static Error Message(this Error error, string message) => error with { Message = message };
    public static Error StackTrace(this Error error, string? stackTrace) => error with { StackTrace = stackTrace };
}