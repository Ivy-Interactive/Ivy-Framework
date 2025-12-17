namespace Ivy.Auth;

public interface IAuthSession
{
    public AuthToken? AuthToken { get; set; }
    public string? AuthSessionData { get; set; }
    public HttpMessageHandler HttpMessageHandler { get; set; }
}

public class AuthSession(HttpMessageHandler httpMessageHandler, AuthToken? authToken = null, string? authSessionData = null) : IAuthSession
{
    public AuthToken? AuthToken { get; set; } = authToken;
    public string? AuthSessionData { get; set; } = authSessionData;
    public HttpMessageHandler HttpMessageHandler { get; set; } = httpMessageHandler;
}

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
