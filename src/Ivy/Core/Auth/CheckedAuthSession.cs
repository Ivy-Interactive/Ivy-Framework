#if DEBUG
namespace Ivy.Core.Auth;

public enum AuthSessionProperty
{
    AuthToken,
    AuthSessionData,
    OAuthProviderSessions
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

    public CheckedAuthTokenHandlerSessionBuilder WithOAuthProviderSessionsAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.OAuthProviderSessions, accessMode);

    public IAuthTokenHandlerSession Build()
    {
        if (_innerAuthSession is IAuthProviderSession providerSession)
        {
            return new CheckedAuthSession(providerSession, _propertyAccessModes);
        }
        return new CheckedAuthTokenHandlerSession(_innerAuthSession, _propertyAccessModes);
    }
}

public class CheckedAuthSessionBuilder(IAuthProviderSession innerAuthSession)
{
    private readonly IAuthProviderSession _innerAuthSession = innerAuthSession;
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

    public CheckedAuthSessionBuilder WithOAuthProviderSessionsAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.OAuthProviderSessions, accessMode);

    public IAuthProviderSession Build()
    {
        return new CheckedAuthSession(_innerAuthSession, _propertyAccessModes);
    }
}

public class CheckedAuthTokenHandlerSession(IAuthTokenHandlerSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes) : IAuthTokenHandlerSession
{
    protected readonly IAuthTokenHandlerSession _innerAuthSession = innerAuthSession;
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
}

public class CheckedAuthSession(IAuthProviderSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes)
    : CheckedAuthTokenHandlerSession(innerAuthSession, propertyAccessModes), IAuthProviderSession
{
    private readonly IAuthProviderSession _innerProviderSession = innerAuthSession;

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> OAuthProviderSessions
    {
        get
        {
            CheckRead(AuthSessionProperty.OAuthProviderSessions);
            return _innerProviderSession.OAuthProviderSessions;
        }
    }

    public void AddOAuthProviderSession(string provider, IAuthTokenHandlerSession session)
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerProviderSession.AddOAuthProviderSession(provider, session);
    }

    public void RemoveOAuthProviderSession(string provider)
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerProviderSession.RemoveOAuthProviderSession(provider);
    }

    public void ClearOAuthProviderSessions()
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerProviderSession.ClearOAuthProviderSessions();
    }

    public HttpMessageHandler HttpMessageHandler
    {
        get => _innerProviderSession.HttpMessageHandler;
        set => _innerProviderSession.HttpMessageHandler = value;
    }

    public event Action<string>? OAuthProviderSessionAdded
    {
        add => _innerProviderSession.OAuthProviderSessionAdded += value;
        remove => _innerProviderSession.OAuthProviderSessionAdded -= value;
    }

    public event Action<string>? OAuthProviderSessionRemoved
    {
        add => _innerProviderSession.OAuthProviderSessionRemoved += value;
        remove => _innerProviderSession.OAuthProviderSessionRemoved -= value;
    }
}
#endif
