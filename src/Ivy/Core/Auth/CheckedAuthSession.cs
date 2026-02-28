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

public readonly struct CheckedAuthSession(IAuthProviderSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes) : IAuthProviderSession
{
    private readonly IAuthProviderSession _innerAuthSession = innerAuthSession;
    private readonly Dictionary<AuthSessionProperty, AuthSessionAccessMode> _propertyAccessModes = propertyAccessModes;

    readonly void CheckRead(AuthSessionProperty property)
    {
        if (!_propertyAccessModes.TryGetValue(property, out var mode) || (mode != AuthSessionAccessMode.ReadOnly && mode != AuthSessionAccessMode.ReadWrite))
        {
            throw new InvalidOperationException($"Read access to '{property}' is not allowed in this context.");
        }
    }

    readonly void CheckWrite(AuthSessionProperty property)
    {
        if (!_propertyAccessModes.TryGetValue(property, out var mode) || (mode != AuthSessionAccessMode.WriteOnly && mode != AuthSessionAccessMode.ReadWrite))
        {
            throw new InvalidOperationException($"Write access to '{property}' is not allowed in this context.");
        }
    }

    public readonly AuthToken? AuthToken
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

    public readonly string? AuthSessionData
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

    public readonly IReadOnlyDictionary<OAuthProvider, IAuthTokenHandlerSession> OAuthProviderSessions
    {
        get
        {
            CheckRead(AuthSessionProperty.OAuthProviderSessions);
            return _innerAuthSession.OAuthProviderSessions;
        }
    }

    public readonly void AddOAuthProviderSession(OAuthProvider provider, IAuthTokenHandlerSession session)
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerAuthSession.AddOAuthProviderSession(provider, session);
    }

    public readonly void RemoveOAuthProviderSession(OAuthProvider provider)
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerAuthSession.RemoveOAuthProviderSession(provider);
    }

    public readonly void ClearOAuthProviderSessions()
    {
        CheckWrite(AuthSessionProperty.OAuthProviderSessions);
        _innerAuthSession.ClearOAuthProviderSessions();
    }

    public readonly HttpMessageHandler HttpMessageHandler
    {
        get => _innerAuthSession.HttpMessageHandler;
        set => _innerAuthSession.HttpMessageHandler = value;
    }

    public event Action<OAuthProvider>? OAuthProviderSessionAdded
    {
        add => _innerAuthSession.OAuthProviderSessionAdded += value;
        remove => _innerAuthSession.OAuthProviderSessionAdded -= value;
    }

    public event Action<OAuthProvider>? OAuthProviderSessionRemoved
    {
        add => _innerAuthSession.OAuthProviderSessionRemoved += value;
        remove => _innerAuthSession.OAuthProviderSessionRemoved -= value;
    }

}
#endif
