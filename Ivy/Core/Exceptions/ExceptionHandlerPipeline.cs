namespace Ivy.Core.Exceptions;

/// <summary>
/// Builder for creating a pipeline of exception handlers that process exceptions in sequence.
/// </summary>
public class ExceptionHandlerPipeline
{
    private readonly List<IExceptionHandler> _handlers = new();

    /// <summary>
    /// Adds an exception handler to the pipeline.
    /// </summary>
    /// <param name="handler">The exception handler to add.</param>
    /// <returns>The pipeline instance for method chaining.</returns>
    public ExceptionHandlerPipeline Use(IExceptionHandler handler)
    {
        _handlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Adds a delegate-based exception handler to the pipeline.
    /// </summary>
    /// <param name="handlerFunc">The function that handles exceptions.</param>
    /// <returns>The pipeline instance for method chaining.</returns>
    public ExceptionHandlerPipeline Use(Func<Exception, bool> handlerFunc)
    {
        _handlers.Add(new DelegateExceptionHandler(handlerFunc));
        return this;
    }

    /// <summary>
    /// Builds the exception handler pipeline into a composite handler.
    /// </summary>
    /// <returns>A composite exception handler that executes all registered handlers in sequence.</returns>
    public IExceptionHandler Build()
    {
        return new CompositeExceptionHandler(_handlers);
    }

    /// <summary>
    /// Internal wrapper for delegate-based exception handlers.
    /// </summary>
    private class DelegateExceptionHandler(Func<Exception, bool> handlerFunc) : IExceptionHandler
    {
        public bool HandleException(Exception exception) => handlerFunc(exception);
    }

    /// <summary>
    /// Composite exception handler that executes multiple handlers in sequence.
    /// </summary>
    private class CompositeExceptionHandler(IEnumerable<IExceptionHandler> handlers) : IExceptionHandler
    {
        /// <summary>
        /// Handles exceptions by executing all registered handlers until one handles the exception.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <returns>True if any handler processed the exception; false otherwise.</returns>
        public bool HandleException(Exception exception)
        {
            foreach (var handler in handlers)
            {
                if (handler.HandleException(exception))
                    return true;
            }
            return false;
        }
    }
}