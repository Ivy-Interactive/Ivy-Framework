using Ivy.Core.HttpTunneling;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions { get; }

    public void AddBrokeredSession(string provider, IAuthTokenHandlerSession session);
    public void RemoveBrokeredSession(string provider);
    public void ClearBrokeredSessions();

    public event Action<string>? BrokeredSessionAdded;
    public event Action<string>? BrokeredSessionRemoved;
}

public class AuthSession(
    AuthToken? authToken = null,
    string? authSessionData = null,
    TunneledHttpMessageHandler? httpMessageHandler = null,
    Dictionary<string, IAuthTokenHandlerSession>? brokeredSessions = null) : AuthTokenHandlerSession(authToken, authSessionData, httpMessageHandler), IAuthSession
{
    private readonly Dictionary<string, IAuthTokenHandlerSession> _brokeredSessions = brokeredSessions ?? [];

    [ActivatorUtilitiesConstructor]
    public AuthSession(TunneledHttpMessageHandler? httpMessageHandler = null) : this(null, null, httpMessageHandler)
    {
    }

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions => _brokeredSessions;

    public event Action<string>? BrokeredSessionAdded;
    public event Action<string>? BrokeredSessionRemoved;

    public void AddBrokeredSession(string provider, IAuthTokenHandlerSession session)
    {
        var isNew = !_brokeredSessions.ContainsKey(provider);
        _brokeredSessions[provider] = session;
        if (isNew)
        {
            BrokeredSessionAdded?.Invoke(provider);
        }
    }

    public void RemoveBrokeredSession(string provider)
    {
        if (_brokeredSessions.Remove(provider))
        {
            BrokeredSessionRemoved?.Invoke(provider);
        }
    }

    public void ClearBrokeredSessions()
    {
        var providers = _brokeredSessions.Keys.ToList();
        _brokeredSessions.Clear();
        foreach (var provider in providers)
        {
            BrokeredSessionRemoved?.Invoke(provider);
        }
    }
}

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
