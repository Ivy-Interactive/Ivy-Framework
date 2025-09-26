using Ivy.Client;

namespace Ivy.Core.Exceptions;

/// <summary>
/// Exception handler that sends errors to the client for display in the UI.
/// </summary>
/// <param name="clientProvider">The client provider used to send error notifications.</param>
public class ClientExceptionHandler(ClientProvider clientProvider) : IExceptionHandler
{
    /// <summary>
    /// Handles exceptions by sending them to the client as error notifications.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>False, allowing the exception to continue through the pipeline.</returns>
    public bool HandleException(Exception exception)
    {
        clientProvider.Error(exception);
        return false;
    }
}