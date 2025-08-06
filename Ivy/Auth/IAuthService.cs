using Ivy.Core;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public interface IAuthService
{
    Task<AuthToken?> LoginAsync(string email, string password);

    public bool ShouldUseUnifiedOAuthFlow();

    Task<AuthToken?> LoginAsync(IClientProvider client, AuthOption option);

    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback);

    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);

    Task LogoutAsync();

    Task<UserInfo?> GetUserInfoAsync();

    AuthOption[] GetAuthOptions();
}