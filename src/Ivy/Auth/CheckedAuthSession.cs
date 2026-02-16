#if DEBUG
namespace Ivy.Auth;

public enum AuthSessionProperty
{
    AccessToken,
    RefreshToken,
    AuthSessionData
}

public enum AuthSessionAccessMode
{
    ReadOnly,
    WriteOnly,
    ReadWrite,
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

    public CheckedAuthSessionBuilder WithAccessTokenAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AccessToken, accessMode);

    public CheckedAuthSessionBuilder WithRefreshTokenAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.RefreshToken, accessMode);

    public CheckedAuthSessionBuilder WithSessionDataAccess(AuthSessionAccessMode accessMode)
        => WithAccessMode(AuthSessionProperty.AuthSessionData, accessMode);

    public IAuthSession Build()
    {
        return new CheckedAuthSession(_innerAuthSession, _propertyAccessModes);
    }
}

public readonly struct CheckedAuthSession(IAuthSession innerAuthSession, Dictionary<AuthSessionProperty, AuthSessionAccessMode> propertyAccessModes) : IAuthSession
{
    private readonly IAuthSession _innerAuthSession = innerAuthSession;
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

    public readonly string? AccessToken
    {
        get
        {
            CheckRead(AuthSessionProperty.AccessToken);
            return _innerAuthSession.AccessToken;
        }
        set
        {
            CheckWrite(AuthSessionProperty.AccessToken);
            _innerAuthSession.AccessToken = value;
        }
    }

    public readonly string? RefreshToken
    {
        get
        {
            CheckRead(AuthSessionProperty.RefreshToken);
            return _innerAuthSession.RefreshToken;
        }
        set
        {
            CheckWrite(AuthSessionProperty.RefreshToken);
            _innerAuthSession.RefreshToken = value;
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

    public readonly HttpMessageHandler HttpMessageHandler
    {
        get => _innerAuthSession.HttpMessageHandler;
        set => _innerAuthSession.HttpMessageHandler = value;
    }
}
#endif