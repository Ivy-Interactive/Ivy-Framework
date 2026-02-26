// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthSession
{
    public AuthToken? AuthToken { get; set; }
    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; }
    public string? AuthSessionData { get; set; }
    public HttpMessageHandler HttpMessageHandler { get; set; }
}

public class AuthSession(HttpMessageHandler httpMessageHandler, AuthToken? authToken = null, Dictionary<OAuthProvider, OAuthProviderToken>? oauthProviderTokens = null, string? authSessionData = null) : IAuthSession
{
    private readonly Dictionary<OAuthProvider, OAuthProviderToken> _oauthProviderTokens = oauthProviderTokens ?? [];

    public AuthToken? AuthToken { get; set; } = authToken;
    public IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get => _oauthProviderTokens; }
    public string? AuthSessionData { get; set; } = authSessionData;
    public HttpMessageHandler HttpMessageHandler { get; set; } = httpMessageHandler;
}

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> OAuthProviderTokens { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
