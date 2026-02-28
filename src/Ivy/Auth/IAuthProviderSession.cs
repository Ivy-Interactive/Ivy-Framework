// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthProviderSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<OAuthProvider, IAuthTokenHandlerSession> OAuthProviderSessions { get; }
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public void AddOAuthProviderSession(OAuthProvider provider, IAuthTokenHandlerSession session);
    public void RemoveOAuthProviderSession(OAuthProvider provider);
    public void ClearOAuthProviderSessions();

    public event Action<OAuthProvider>? OAuthProviderSessionAdded;
    public event Action<OAuthProvider>? OAuthProviderSessionRemoved;
}

public class AuthProviderSession : AuthTokenHandlerSession, IAuthProviderSession
{
    private readonly Dictionary<OAuthProvider, IAuthTokenHandlerSession> _oauthProviderSessions;

    public AuthProviderSession(
        HttpMessageHandler httpMessageHandler,
        AuthToken? authToken = null,
        Dictionary<OAuthProvider, IAuthTokenHandlerSession>? oauthProviderSessions = null,
        string? authSessionData = null)
        : base(authToken, authSessionData)
    {
        HttpMessageHandler = httpMessageHandler;
        _oauthProviderSessions = oauthProviderSessions ?? [];
    }

    public IReadOnlyDictionary<OAuthProvider, IAuthTokenHandlerSession> OAuthProviderSessions => _oauthProviderSessions;
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public event Action<OAuthProvider>? OAuthProviderSessionAdded;
    public event Action<OAuthProvider>? OAuthProviderSessionRemoved;

    public void AddOAuthProviderSession(OAuthProvider provider, IAuthTokenHandlerSession session)
    {
        var isNew = !_oauthProviderSessions.ContainsKey(provider);
        _oauthProviderSessions[provider] = session;
        if (isNew)
        {
            OAuthProviderSessionAdded?.Invoke(provider);
        }
    }

    public void RemoveOAuthProviderSession(OAuthProvider provider)
    {
        if (_oauthProviderSessions.Remove(provider))
        {
            OAuthProviderSessionRemoved?.Invoke(provider);
        }
    }

    public void ClearOAuthProviderSessions()
    {
        var providers = _oauthProviderSessions.Keys.ToList();
        _oauthProviderSessions.Clear();
        foreach (var provider in providers)
        {
            OAuthProviderSessionRemoved?.Invoke(provider);
        }
    }
}

public readonly struct AuthProviderSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<OAuthProvider, IAuthTokenHandlerSession> OAuthProviderSessions { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
