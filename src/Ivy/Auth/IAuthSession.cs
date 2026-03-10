// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthSessions { get; }
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public void AddOAuthSession(string provider, IAuthTokenHandlerSession session);
    public void RemoveOAuthSession(string provider);
    public void ClearOAuthSessions();

    public event Action<string>? OAuthSessionAdded;
    public event Action<string>? OAuthSessionRemoved;
}

public class AuthSession : AuthTokenHandlerSession, IAuthSession
{
    private readonly Dictionary<string, IAuthTokenHandlerSession> _oauthSessions;

    public AuthSession(
        HttpMessageHandler httpMessageHandler,
        AuthToken? authToken = null,
        Dictionary<string, IAuthTokenHandlerSession>? oauthSessions = null,
        string? authSessionData = null)
        : base(authToken, authSessionData)
    {
        HttpMessageHandler = httpMessageHandler;
        _oauthSessions = oauthSessions ?? [];
    }

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthSessions => _oauthSessions;
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public event Action<string>? OAuthSessionAdded;
    public event Action<string>? OAuthSessionRemoved;

    public void AddOAuthSession(string provider, IAuthTokenHandlerSession session)
    {
        var isNew = !_oauthSessions.ContainsKey(provider);
        _oauthSessions[provider] = session;
        if (isNew)
        {
            OAuthSessionAdded?.Invoke(provider);
        }
    }

    public void RemoveOAuthSession(string provider)
    {
        if (_oauthSessions.Remove(provider))
        {
            OAuthSessionRemoved?.Invoke(provider);
        }
    }

    public void ClearOAuthSessions()
    {
        var providers = _oauthSessions.Keys.ToList();
        _oauthSessions.Clear();
        foreach (var provider in providers)
        {
            OAuthSessionRemoved?.Invoke(provider);
        }
    }
}

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthSessions { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
