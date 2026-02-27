// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthProviderSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; }
    public HttpMessageHandler HttpMessageHandler { get; set; }

    public void AddOAuthProviderToken(OAuthProviderToken token);
    public void RemoveOAuthProviderToken(OAuthProvider provider);
    public void ClearOAuthProviderTokens();
}

public class AuthProviderSession(HttpMessageHandler httpMessageHandler, AuthToken? authToken = null, Dictionary<OAuthProvider, OAuthProviderToken>? oauthProviderTokens = null, string? authSessionData = null) : AuthTokenHandlerSession(authToken, authSessionData), IAuthProviderSession
{
    private readonly Dictionary<OAuthProvider, OAuthProviderToken> _oauthProviderTokens = oauthProviderTokens ?? [];

    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get => _oauthProviderTokens; }
    public HttpMessageHandler HttpMessageHandler { get; set; } = httpMessageHandler;

    public void AddOAuthProviderToken(OAuthProviderToken token)
    {
        _oauthProviderTokens[token.Provider] = token;
    }

    public void RemoveOAuthProviderToken(OAuthProvider provider)
    {
        _oauthProviderTokens.Remove(provider);
    }

    public void ClearOAuthProviderTokens()
    {
        _oauthProviderTokens.Clear();
    }
}

public readonly struct AuthProviderSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
