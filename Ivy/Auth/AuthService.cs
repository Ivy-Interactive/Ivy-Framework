using Ivy.Helpers;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, IAuthSession authSession) : IAuthService
{
    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LoginAsync(authSession, email, password, ct), cancellationToken);
        authSession.AuthToken = token;
        return token;
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        return TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetOAuthUriAsync(authSession, option, callback, ct), cancellationToken);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var token = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.HandleOAuthCallbackAsync(authSession, request, ct), cancellationToken);
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

        await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.LogoutAsync(authSession, ct), cancellationToken);
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

        return await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.GetUserInfoAsync(authSession, ct), cancellationToken);
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

        var refreshedToken = await TimeoutHelper.WithTimeoutAsync(ct =>
            authProvider.RefreshAccessTokenAsync(authSession, ct), cancellationToken);
        authSession.AuthToken = refreshedToken;
        return refreshedToken;
    }

    public AuthToken? GetCurrentToken() => authSession.AuthToken;

    public string? GetCurrentSessionData() => authSession.AuthSessionData;

    public IAuthSession GetAuthSession() => authSession;
}
