namespace Ivy.Auth;

public interface IAuthSession
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? AuthSessionData { get; set; }
    public HttpMessageHandler HttpMessageHandler { get; set; }
}

public class AuthSession(HttpMessageHandler httpMessageHandler, string? accessToken = null, string? refreshToken = null, string? authSessionData = null) : IAuthSession
{
    public string? AccessToken { get; set; } = accessToken;
    public string? RefreshToken { get; set; } = refreshToken;
    public string? AuthSessionData { get; set; } = authSessionData;
    public HttpMessageHandler HttpMessageHandler { get; set; } = httpMessageHandler;
}

public readonly struct AuthSessionSnapshot
{
    public readonly string? AccessToken { get; init; }
    public readonly string? RefreshToken { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
