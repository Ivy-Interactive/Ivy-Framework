#if DEBUG
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public class CheckedAuthProvider(IAuthProvider innerAuthProvider) : IAuthProvider
{
    private readonly IAuthProvider _innerAuthProvider = innerAuthProvider;

    public Task InitializeAsync(IAuthSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadWrite)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.InitializeAsync(authSession, requestScheme, requestHost, cancellationToken);
    }

    public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.LoginAsync(authSession, email, password, cancellationToken);
    }

    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.LogoutAsync(authSession, cancellationToken);
    }

    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.RefreshAccessTokenAsync(authSession, cancellationToken);
    }

    public Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.ValidateAccessTokenAsync(authSession, cancellationToken);
    }

    public Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.GetUserInfoAsync(authSession, cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
        => _innerAuthProvider.GetAuthOptions();

    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.GetOAuthUriAsync(authSession, option, callback, cancellationToken);
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadWrite)
            .Build();
        return _innerAuthProvider.HandleOAuthCallbackAsync(authSession, request, cancellationToken);
    }

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        if (authSession.AuthToken?.AccessToken == null)
        {
            throw new InvalidOperationException("AuthSession.AuthToken.AccessToken is null");
        }

        authSession = authSession.WithCheckedAccess()
            .WithTokenAccess(AuthSessionAccessMode.ReadOnly)
            .WithSessionDataAccess(AuthSessionAccessMode.ReadOnly)
            .Build();
        return _innerAuthProvider.GetAccessTokenLifetimeAsync(authSession, cancellationToken);
    }
}
#endif