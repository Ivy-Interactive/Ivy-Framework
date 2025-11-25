using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public class AuthService(IAuthProvider authProvider, AuthToken? token = null) : IAuthService
{
    private volatile AuthToken? _token = token;

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var token = await authProvider.LoginAsync(email, password, cancellationToken);
        _token = token;
        return token;
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        return authProvider.GetOAuthUriAsync(option, callback, cancellationToken);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var token = await authProvider.HandleOAuthCallbackAsync(request, cancellationToken);
        _token = token;
        return token;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var token = Interlocked.Exchange(ref _token, null);

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return;
        }

        await authProvider.LogoutAsync(token.AccessToken, cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken)
    {
        var token = _token;

        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            return null;
        }

        //todo: cache this!

        return await authProvider.GetUserInfoAsync(token.AccessToken, cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
    {
        return authProvider.GetAuthOptions();
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = _token;
        if (token is null)
        {
            return null;
        }

        var refreshedToken = await authProvider.RefreshAccessTokenAsync(token, cancellationToken);

        // Update _token only if it's still the same object we read earlier.
        var seen = Interlocked.CompareExchange(ref _token, refreshedToken, token);
        if (!ReferenceEquals(seen, token))
        {
            // Another thread updated the token in the meantime; return it if valid.
            if (seen is not null && await authProvider.ValidateAccessTokenAsync(seen.AccessToken, cancellationToken))
            {
                return seen;
            }

            // Otherwise, set and return null.
            _token = null;
            return null;
        }

        return refreshedToken;
    }
    public AuthToken? GetCurrentToken()
    {
        return _token;
    }
}
