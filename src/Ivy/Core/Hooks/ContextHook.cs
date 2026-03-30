namespace Ivy.Core.Hooks;

public class ContextHook
{
    public int Identity { get; }
    public Type ContextType { get; }
    public object ContextInstance { get; private set; }
    private IDisposable? _disposable;

    public ContextHook(int identity, Type contextType, object contextInstance)
    {
        Identity = identity;
        ContextType = contextType;
        ContextInstance = contextInstance;
        if (contextInstance is IDisposable disposable)
            _disposable = disposable;
    }

    public void UpdateInstance(object newInstance)
    {
        ContextInstance = newInstance;
        if (newInstance is IDisposable disposable)
            _disposable = disposable;
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
