using Ivy.Client;

namespace Ivy.Core.Exceptions;

public class ClientExceptionHandler(ClientProvider clientProvider) : IExceptionHandler
{
    public bool HandleException(Exception exception)
    {
        clientProvider.Error(exception);
        return false;
    }
}