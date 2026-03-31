using Ivy.Core.HttpTunneling;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthTokenHandlerSession
{
    public AuthToken? AuthToken { get; set; }
    public string? AuthSessionData { get; set; }
    public TunneledHttpMessageHandler? TunneledHttpMessageHandler { get; set; }
}

public class AuthTokenHandlerSession(AuthToken? authToken = null, string? authSessionData = null, TunneledHttpMessageHandler? httpMessageHandler = null) : IAuthTokenHandlerSession
{
    public AuthToken? AuthToken { get; set; } = authToken;
    public string? AuthSessionData { get; set; } = authSessionData;
    public TunneledHttpMessageHandler? TunneledHttpMessageHandler { get; set; } = httpMessageHandler;

    [ActivatorUtilitiesConstructor]
    public AuthTokenHandlerSession(TunneledHttpMessageHandler? httpMessageHandler = null) : this(null, null, httpMessageHandler)
    {
    }
}

public readonly struct AuthTokenHandlerSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
