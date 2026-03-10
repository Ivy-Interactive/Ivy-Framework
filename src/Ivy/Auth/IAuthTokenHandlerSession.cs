// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthTokenHandlerSession
{
    public AuthToken? AuthToken { get; set; }
    public string? AuthSessionData { get; set; }
}

public class AuthTokenHandlerSession(AuthToken? authToken = null, string? authSessionData = null) : IAuthTokenHandlerSession
{
    public AuthToken? AuthToken { get; set; } = authToken;
    public string? AuthSessionData { get; set; } = authSessionData;
}

public readonly struct AuthTokenHandlerSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
