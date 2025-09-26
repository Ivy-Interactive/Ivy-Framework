namespace Ivy.Core.Exceptions;

/// <summary>
/// Exception handler that logs errors to the console output.
/// </summary>
public class ConsoleExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Handles exceptions by writing error messages to the console.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>False, allowing the exception to continue through the pipeline.</returns>
    public bool HandleException(Exception exception)
    {
        var inner = exception.InnerException ?? exception;
        Console.WriteLine($"Error: {inner.Message}");
        return false;
    }
}