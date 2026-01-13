namespace Ivy.Core.Exceptions;

public class ConsoleExceptionHandler : IExceptionHandler
{
    public bool HandleException(Exception exception)
    {
        var inner = exception.InnerException ?? exception;
        Console.WriteLine($"Error: {inner.Message}");
        return false;
    }
}