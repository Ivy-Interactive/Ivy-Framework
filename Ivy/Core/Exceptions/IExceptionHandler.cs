namespace Ivy.Core.Exceptions;

/// <summary>
/// Interface for handling exceptions in the Ivy framework.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Handles the specified exception.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>True if the exception was handled and should not be propagated further; false otherwise.</returns>
    public bool HandleException(Exception exception);
}