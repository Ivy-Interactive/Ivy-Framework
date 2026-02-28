#if DEBUG
using Ivy.Core;
using Microsoft.AspNetCore.Http;

namespace Ivy.Core.Auth;

public class CheckedAuthProvider(IAuthProvider innerAuthProvider) : IAuthProvider
{
    private readonly IAuthProvider _innerAuthProvider = innerAuthProvider;

    public Task InitializeAsync(IAuthProviderSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadWrite)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.InitializeAsync(authSession, requestScheme, requestHost, cancellationToken);
    }

    public Task<AuthToken?> LoginAsync(IAuthProviderSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.LoginAsync(authSession, email, password, cancellationToken);
    }

    public Task LogoutAsync(IAuthProviderSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.LogoutAsync(authSession, cancellationToken);
    }

    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = ((IAuthProviderSession)authSession).WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.RefreshAccessTokenAsync(checkedSession, cancellationToken);
    }

    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = ((IAuthProviderSession)authSession).WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.ValidateAccessTokenAsync(checkedSession, cancellationToken);
    }

    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = ((IAuthProviderSession)authSession).WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.GetUserInfoAsync(checkedSession, cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
        => _innerAuthProvider.GetAuthOptions();

    public Task<Uri> GetOAuthUriAsync(IAuthProviderSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.GetOAuthUriAsync(authSession, option, callback, cancellationToken);
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthProviderSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.HandleOAuthCallbackAsync(authSession, request, cancellationToken);
    }

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        var checkedSession = ((IAuthProviderSession)authSession).WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.GetAccessTokenLifetimeAsync(checkedSession, cancellationToken);
    }

    public Task<Dictionary<OAuthProvider, IAuthTokenHandlerSession>?> GetOAuthProviderSessionsAsync(IAuthProviderSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .WithOAuthProviderSessionsAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.GetOAuthProviderSessionsAsync(authSession, cancellationToken);
    }
}
#endif
