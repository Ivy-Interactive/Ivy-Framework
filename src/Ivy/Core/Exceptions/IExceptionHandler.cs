namespace Ivy.Core.Exceptions;

public interface IExceptionHandler
{
    public bool HandleException(Exception exception);
}