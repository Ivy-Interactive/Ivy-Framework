namespace Ivy.Auth;

public interface IAuthSession
{
    public AuthToken? AuthToken { get; set; }
    public string? AuthSessionData { get; set; }
}

public class AuthSession(AuthToken? authToken = null, string? authSessionData = null) : IAuthSession
{
    public AuthToken? AuthToken { get; set; } = authToken;
    public string? AuthSessionData { get; set; } = authSessionData;
}
