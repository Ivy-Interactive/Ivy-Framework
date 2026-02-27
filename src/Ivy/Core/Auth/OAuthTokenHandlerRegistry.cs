namespace Ivy.Core.Auth;

public class OAuthTokenHandlerRegistry : IOAuthTokenHandlerRegistry
{
    private readonly Dictionary<OAuthProvider, IAuthTokenHandler> _handlers = new();

    public void Register(OAuthProvider provider, IAuthTokenHandler handler)
    {
        _handlers[provider] = handler;
    }

    public IAuthTokenHandler? GetHandler(OAuthProvider provider)
    {
        return _handlers.TryGetValue(provider, out var handler) ? handler : null;
    }

    public IEnumerable<OAuthProvider> GetRegisteredProviders()
    {
        return _handlers.Keys;
    }
}
