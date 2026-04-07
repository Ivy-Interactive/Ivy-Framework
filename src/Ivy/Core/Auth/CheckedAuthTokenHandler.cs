#if DEBUG
namespace Ivy.Core.Auth;

public class CheckedAuthTokenHandler(IAuthTokenHandler innerAuthTokenHandler) : IAuthTokenHandler
{
    protected readonly IAuthTokenHandler _innerAuthTokenHandler = innerAuthTokenHandler;

    public Task InitializeAsync(IAuthTokenHandlerSession authSession, string requestScheme, string requestHost, string? basePath = null, CancellationToken cancellationToken = default)
    {
        var checkedSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadWrite)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithBrokeredSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthTokenHandler.InitializeAsync(checkedSession, requestScheme, requestHost, basePath, cancellationToken);
    }

    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithBrokeredSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthTokenHandler.RefreshAccessTokenAsync(checkedSession, cancellationToken);
    }

    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthTokenHandler.ValidateAccessTokenAsync(checkedSession, cancellationToken);
    }

    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .WithBrokeredSessionsAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthTokenHandler.GetUserInfoAsync(checkedSession, cancellationToken);
    }

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .WithBrokeredSessionsAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthTokenHandler.GetAccessTokenLifetimeAsync(checkedSession, cancellationToken);
    }
}
#endif
