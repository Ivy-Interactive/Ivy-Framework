#if DEBUG
using Ivy.Core.HttpTunneling;

namespace Ivy.Core.Auth;

public enum AuthSessionProperty
{
    AuthToken,
    AuthSessionData,
    BrokeredSessions
}

public enum AuthSessionAccessMode
{
    ReadOnly,
    WriteOnly,
    ReadWrite,
}

public class CheckedAuthTokenHandlerSessionBuilder(IAuthTokenHandlerSession innerAuthSession)
{
    private readonly IAuthTokenHandlerSession _innerAuthSession = innerAuthSession;
    private readonly Dictionary<AuthSessionProperty, AuthSessionAccessMode> _propertyAccessModes = [];

    public CheckedAuthTokenHandlerSessionBuilder WithAccessMode(AuthSessionProperty property, AuthSessionAccessMode accessMode)
    {
        _propertyAccessModes[property] = accessMode;
        return this;
    }

    public CheckedAuthTokenHandlerSessionBuilder WithTokenAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AuthToken, accessMode);

    public CheckedAuthTokenHandlerSessionBuilder WithSessionDataAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AuthSessionData, accessMode);

    public CheckedAuthTokenHandlerSessionBuilder WithBrokeredSessionsAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.BrokeredSessions, accessMode);

    public IAuthTokenHandlerSession Build()
    {
        if (_innerAuthSession is IAuthSession providerSession)
        {
            return new CheckedAuthSession(providerSession, _propertyAccessModes);
        }
        return new CheckedAuthTokenHandlerSession(_innerAuthSession, _propertyAccessModes);
    }
}

public class CheckedAuthSessionBuilder(IAuthSession innerAuthSession)
{
    private readonly IAuthSession _innerAuthSession = innerAuthSession;
    private readonly Dictionary<AuthSessionProperty, AuthSessionAccessMode> _propertyAccessModes = [];

    public CheckedAuthSessionBuilder WithAccessMode(AuthSessionProperty property, AuthSessionAccessMode accessMode)
    {
        _propertyAccessModes[property] = accessMode;
        return this;
    }

    public CheckedAuthSessionBuilder WithTokenAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AuthToken, accessMode);

    public CheckedAuthSessionBuilder WithSessionDataAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AuthSessionData, accessMode);

    public CheckedAuthSessionBuilder WithBrokeredSessionsAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.BrokeredSessions, accessMode);

    public IAuthSession Build()
    {
        return new CheckedAuthSession(_innerAuthSession, _propertyAccessModes);
    }
}

public class CheckedAuthTokenHandlerSession(IAuthTokenHandlerSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes) : IAuthTokenHandlerSession
{
    private readonly IAuthTokenHandlerSession _innerAuthSession = innerAuthSession;
    protected readonly Dictionary<AuthSessionProperty, AuthSessionAccessMode> _propertyAccessModes = propertyAccessModes;

    protected void CheckRead(AuthSessionProperty property)
    {
        if (!_propertyAccessModes.TryGetValue(property, out var mode) || (mode != AuthSessionAccessMode.ReadOnly && mode != AuthSessionAccessMode.ReadWrite))
        {
            throw new InvalidOperationException($"Read access to '{property}' is not allowed in this context.");
        }
    }

    protected void CheckWrite(AuthSessionProperty property)
    {
        if (!_propertyAccessModes.TryGetValue(property, out var mode) || (mode != AuthSessionAccessMode.WriteOnly && mode != AuthSessionAccessMode.ReadWrite))
        {
            throw new InvalidOperationException($"Write access to '{property}' is not allowed in this context.");
        }
    }

    public AuthToken? AuthToken
    {
        get
        {
            CheckRead(AuthSessionProperty.AuthToken);
            return _innerAuthSession.AuthToken;
        }
        set
        {
            CheckWrite(AuthSessionProperty.AuthToken);
            _innerAuthSession.AuthToken = value;
        }
    }

    public string? AuthSessionData
    {
        get
        {
            CheckRead(AuthSessionProperty.AuthSessionData);
            return _innerAuthSession.AuthSessionData;
        }
        set
        {
            CheckWrite(AuthSessionProperty.AuthSessionData);
            _innerAuthSession.AuthSessionData = value;
        }
    }

    public TunneledHttpMessageHandler? TunneledHttpMessageHandler
    {
        get => _innerAuthSession.TunneledHttpMessageHandler;
        set => _innerAuthSession.TunneledHttpMessageHandler = value;
    }
}

public class CheckedAuthSession(IAuthSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes)
    : CheckedAuthTokenHandlerSession(innerAuthSession, propertyAccessModes), IAuthSession
{
    private readonly IAuthSession _innerAuthSession = innerAuthSession;

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions
    {
        get
        {
            CheckRead(AuthSessionProperty.BrokeredSessions);
            return _innerAuthSession.BrokeredSessions;
        }
    }

    public void AddBrokeredSession(string provider, IAuthTokenHandlerSession session)
    {
        CheckWrite(AuthSessionProperty.BrokeredSessions);
        _innerAuthSession.AddBrokeredSession(provider, session);
    }

    public void RemoveBrokeredSession(string provider)
    {
        CheckWrite(AuthSessionProperty.BrokeredSessions);
        _innerAuthSession.RemoveBrokeredSession(provider);
    }

    public void ClearBrokeredSessions()
    {
        CheckWrite(AuthSessionProperty.BrokeredSessions);
        _innerAuthSession.ClearBrokeredSessions();
    }

    public event Action<string>? BrokeredSessionAdded
    {
        add => _innerAuthSession.BrokeredSessionAdded += value;
        remove => _innerAuthSession.BrokeredSessionAdded -= value;
    }

    public event Action<string>? BrokeredSessionRemoved
    {
        add => _innerAuthSession.BrokeredSessionRemoved += value;
        remove => _innerAuthSession.BrokeredSessionRemoved -= value;
    }
}
#endif
