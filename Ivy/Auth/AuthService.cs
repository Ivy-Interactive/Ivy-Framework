using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, IAuthSession authSession) : IAuthService
{
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var token = await authProvider.LoginAsync(authSession, email, password, cancellationToken);
        authSession.AuthToken = token;
        return token;
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        return authProvider.GetOAuthUriAsync(authSession, option, callback, cancellationToken);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var token = await authProvider.HandleOAuthCallbackAsync(authSession, request, cancellationToken);
        authSession.AuthToken = token;
        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var token = authSession.AuthToken;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return;
        }

        await authProvider.LogoutAsync(authSession, cancellationToken);
        authSession.AuthToken = null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var token = authSession.AuthToken;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        //todo: cache this!

        return await authProvider.GetUserInfoAsync(authSession, cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = authSession.AuthToken;
        if (token is null)
        {
            return null;
        }

        var refreshedToken = await authProvider.RefreshAccessTokenAsync(authSession, cancellationToken);
        authSession.AuthToken = refreshedToken;
        return refreshedToken;
    }

    public AuthToken? GetCurrentToken() => authSession.AuthToken;

    public string? GetCurrentSessionData() => authSession.AuthSessionData;

    public IAuthSession GetAuthSession() => authSession;
}
