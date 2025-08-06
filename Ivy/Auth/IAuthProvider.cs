using Ivy.Client;
using Ivy.Core;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public interface IAuthProvider
{
    Task<AuthToken?> LoginAsync(string email, string password);

    Task LogoutAsync(string jwt);

    Task<AuthToken?> RefreshJwtAsync(AuthToken jwt);

    Task<bool> ValidateJwtAsync(string jwt);

    Task<UserInfo?> GetUserInfoAsync(string jwt);

    AuthOption[] GetAuthOptions();

    public bool ShouldUseUnifiedOAuthFlow() => false;

    Task<AuthToken?> LoginAsync(IClientProvider client, AuthOption option) => throw new InvalidOperationException("unified OAuth flow is not implemented for this provider");

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);

    void SetHttpContext(HttpContext context)
    {
    }
}