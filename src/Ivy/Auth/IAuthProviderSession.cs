// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthProviderSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthProviderSessions { get; }
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public void AddOAuthProviderSession(string provider, IAuthTokenHandlerSession session);
    public void RemoveOAuthProviderSession(string provider);
    public void ClearOAuthProviderSessions();

    public event Action<string>? OAuthProviderSessionAdded;
    public event Action<string>? OAuthProviderSessionRemoved;
}

public class AuthProviderSession : AuthTokenHandlerSession, IAuthProviderSession
{
    private readonly Dictionary<string, IAuthTokenHandlerSession> _oauthProviderSessions;

    public AuthProviderSession(
        HttpMessageHandler httpMessageHandler,
        AuthToken? authToken = null,
        Dictionary<string, IAuthTokenHandlerSession>? oauthProviderSessions = null,
        string? authSessionData = null)
        : base(authToken, authSessionData)
    {
        HttpMessageHandler = httpMessageHandler;
        _oauthProviderSessions = oauthProviderSessions ?? [];
    }

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthProviderSessions => _oauthProviderSessions;
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public event Action<string>? OAuthProviderSessionAdded;
    public event Action<string>? OAuthProviderSessionRemoved;

    public void AddOAuthProviderSession(string provider, IAuthTokenHandlerSession session)
    {
        var isNew = !_oauthProviderSessions.ContainsKey(provider);
        _oauthProviderSessions[provider] = session;
        if (isNew)
        {
            OAuthProviderSessionAdded?.Invoke(provider);
        }
    }

    public void RemoveOAuthProviderSession(string provider)
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
    public readonly IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthProviderSessions { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
