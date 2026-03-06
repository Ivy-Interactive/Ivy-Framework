using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AuthProviderHelpers
{
    /// <summary>
    /// Gets the User-Agent string to use for HTTP requests from auth providers.
    /// Checks configuration for a custom value, otherwise uses Ivy-Framework/{version}.
    /// </summary>
    public static string GetUserAgent(IConfiguration configuration, string configKey)
    {
        var ivyVersion = typeof(IAuthProvider).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        return configuration.GetValue<string>(configKey) ?? $"Ivy-Framework/{ivyVersion}";
    }
}

public interface IAuthProvider : IAuthTokenHandler
{
    Task<AuthToken?> LoginAsync(IAuthProviderSession authSession, string email, string password, CancellationToken cancellationToken = default);

    Task LogoutAsync(IAuthProviderSession authSession, CancellationToken cancellationToken = default);

    AuthOption[] GetAuthOptions();

    Task<Uri> GetOAuthUriAsync(IAuthProviderSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    Task<AuthToken?> HandleOAuthCallbackAsync(IAuthProviderSession authSession, HttpRequest request, CancellationToken cancellationToken = default);

    Task<OAuthProviderSessionsResult> GetOAuthProviderSessionsAsync(IAuthProviderSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OAuthProviderSessionsResult.Failure(canRetry: false));
    }

    bool OpenOAuthLoginInNewTab => false;
}
