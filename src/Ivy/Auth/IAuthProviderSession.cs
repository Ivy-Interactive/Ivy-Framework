// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthProviderSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; }
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public void AddOAuthProviderToken(OAuthProviderToken token);
    public void RemoveOAuthProviderToken(OAuthProvider provider);
    public void ClearOAuthProviderTokens();

    public event Action<OAuthProvider>? OAuthProviderTokenAdded;
    public event Action<OAuthProvider>? OAuthProviderTokenRemoved;
}

public class AuthProviderSession(HttpMessageHandler httpMessageHandler, AuthToken? authToken = null, Dictionary<OAuthProvider, OAuthProviderToken>? oauthProviderTokens = null, string? authSessionData = null) : AuthTokenHandlerSession(authToken, authSessionData), IAuthProviderSession
{
    private readonly Dictionary<OAuthProvider, OAuthProviderToken> _oauthProviderTokens = oauthProviderTokens ?? [];

    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get => _oauthProviderTokens; }
    public HttpMessageHandler HttpMessageHandler { get; set; } = httpMessageHandler;

    public event Action<OAuthProvider>? OAuthProviderTokenAdded;
    public event Action<OAuthProvider>? OAuthProviderTokenRemoved;

    public void AddOAuthProviderToken(OAuthProviderToken token)
    {
        var isNew = !_oauthProviderTokens.ContainsKey(token.Provider);
        _oauthProviderTokens[token.Provider] = token;
        if (isNew)
        {
            OAuthProviderTokenAdded?.Invoke(token.Provider);
        }
    }

    public void RemoveOAuthProviderToken(OAuthProvider provider)
    {
        if (_oauthProviderTokens.Remove(provider))
        {
            OAuthProviderTokenRemoved?.Invoke(provider);
        }
    }

    public void ClearOAuthProviderTokens()
    {
        var providers = _oauthProviderTokens.Keys.ToList();
        _oauthProviderTokens.Clear();
        foreach (var provider in providers)
        {
            OAuthProviderTokenRemoved?.Invoke(provider);
        }
    }
}

public readonly struct AuthProviderSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
