namespace Ivy.Core.Exceptions;

public class ExceptionHandlerPipeline
{
    private readonly List<IExceptionHandler> _handlers = new();

    public ExceptionHandlerPipeline Use(IExceptionHandler handler)
    {
        _handlers.Add(handler);
        return this;
    }

    public ExceptionHandlerPipeline Use(Func<Exception, bool> handlerFunc)
    {
        _handlers.Add(new DelegateExceptionHandler(handlerFunc));
        return this;
    }

    public IExceptionHandler Build()
    {
        return new CompositeExceptionHandler(_handlers);
    }

    private class DelegateExceptionHandler(Func<Exception, bool> handlerFunc) : IExceptionHandler
    {
        public bool HandleException(Exception exception) => handlerFunc(exception);
    }


    private class CompositeExceptionHandler(IEnumerable<IExceptionHandler> handlers) : IExceptionHandler
    {
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