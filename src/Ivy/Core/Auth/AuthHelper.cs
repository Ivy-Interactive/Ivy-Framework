using System.Net;
using System.Text.Json;
using Grpc.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Auth;

public static class AuthHelper
{
    public static AuthSession GetAuthSession(HttpContext context, HttpMessageHandler httpMessageHandler)
    => GetAuthCookies(context) is (var accessToken, var refreshToken, var tag, var authSessionData, var oauthTokens)
        ? GetAuthSession(accessToken, refreshToken, tag, authSessionData, oauthTokens, httpMessageHandler)
        : new AuthSession(httpMessageHandler);

    public static AuthSession GetAuthSession(ServerCallContext context, HttpMessageHandler httpMessageHandler)
    => GetAuthCookies(context) is (var accessToken, var refreshToken, var tag, var authSessionData, var oauthTokens)
        ? GetAuthSession(accessToken, refreshToken, tag, authSessionData, oauthTokens, httpMessageHandler)
        : new AuthSession(httpMessageHandler);

    public static async Task ValidateAuthIfRequired(global::Ivy.Server server, AppSessionStore sessionStore, string connectionId, ServerCallContext context)
    {
        // Check if auth is required
        if (server.AuthProviderType == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(connectionId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ConnectionId is required in the request."));
        }

        if (!sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Connection '{connectionId}' not found."));
        }


        var serviceProvider = session.AppServices;
        var clientProvider = serviceProvider.GetRequiredService<IClientProvider>();
        var httpMessageHandler = serviceProvider.GetRequiredService<HttpMessageHandler>();
        var authSession = GetAuthSession(context, httpMessageHandler);
        try
        {
            await ValidateAuth(serviceProvider, authSession, context.CancellationToken);
        }
        catch (MissingAuthTokenException ex)
        {
            clientProvider.Toast(ex.Message, "Authentication failed");
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (InvalidAuthTokenException ex)
        {
            clientProvider.Toast(ex.Message, "Authentication failed");
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (AuthProviderNotConfiguredException ex)
        {
            clientProvider.Error(ex);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
        catch (AuthValidationException ex)
        {
            clientProvider.Error(ex);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public static async Task<IActionResult?> ValidateAuthIfRequired(this Controller controller, global::Ivy.Server server, IServiceProvider serviceProvider)
    {
        // Check if auth is required
        if (server.AuthProviderType == null)
        {
            return null;
        }

        var clientProvider = serviceProvider.GetRequiredService<IClientProvider>();
        try
        {
            var httpMessageHandler = serviceProvider.GetRequiredService<HttpMessageHandler>();
            var authSession = GetAuthSession(controller.HttpContext, httpMessageHandler);
            await ValidateAuth(serviceProvider, authSession, controller.HttpContext.RequestAborted);
        }
        catch (MissingAuthTokenException ex)
        {
            clientProvider.Toast(ex.Message, "Authentication failed");
            return controller.Unauthorized(ex.Message);
        }
        catch (InvalidAuthTokenException ex)
        {
            clientProvider.Toast(ex.Message, "Authentication failed");
            return controller.Unauthorized(ex.Message);
        }
        catch (AuthProviderNotConfiguredException ex)
        {
            clientProvider.Error(ex);
            return controller.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (AuthValidationException ex)
        {
            clientProvider.Error(ex);
            return controller.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

        return null;
    }

    private static async Task ValidateAuth(IServiceProvider serviceProvider, AuthSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken == null || string.IsNullOrEmpty(authSession.AuthToken.AccessToken))
        {
            throw new MissingAuthTokenException();
        }

        // Get auth provider and validate token
        var authProvider = serviceProvider.GetService<IAuthProvider>()
            ?? throw new AuthProviderNotConfiguredException();

        try
        {
            var isValid = await authProvider.ValidateAccessTokenAsync(authSession, cancellationToken);
            if (!isValid)
            {
                throw new InvalidAuthTokenException();
            }
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthValidationException("Error validating auth token.", ex);
        }
    }

    private static (string? AccessToken, string? RefreshToken, string? Tag, string? AuthSessionData, Dictionary<OAuthProvider, OAuthProviderToken> OAuthTokens) GetAuthCookies(HttpContext context)
    {
        var cookies = context.Request.Cookies;
        var accessToken = cookies["access_token"].NullIfEmpty();
        var refreshToken = cookies["refresh_token"].NullIfEmpty();
        var tag = cookies["auth_tag"].NullIfEmpty();
        var authSessionDataValue = cookies["auth_session_data"].NullIfEmpty();

        var oauthTokens = ExtractOAuthProviderTokensFromCookies(cookies);

        return (accessToken, refreshToken, tag, authSessionDataValue, oauthTokens);
    }

    private static (string? AccessToken, string? RefreshToken, string? Tag, string? AuthSessionData, Dictionary<OAuthProvider, OAuthProviderToken> OAuthTokens) GetAuthCookies(ServerCallContext context)
    {
        var cookies = context.RequestHeaders.GetValue("cookie") ?? string.Empty;
        if (string.IsNullOrEmpty(cookies))
        {
            return (null, null, null, null, new Dictionary<OAuthProvider, OAuthProviderToken>());
        }

        var cookieHeader = CookieHeaderValue.ParseList([cookies]).ToList();

        string? GetCookie(string name)
        {
            var rawValue = cookieHeader
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value.Value;
            return !string.IsNullOrEmpty(rawValue)
                ? WebUtility.UrlDecode(rawValue)
                : null;
        }

        var accessToken = GetCookie("access_token");
        var refreshToken = GetCookie("refresh_token");
        var tag = GetCookie("auth_tag");
        var authSessionDataValue = GetCookie("auth_session_data");

        var oauthTokens = ExtractOAuthProviderTokensFromCookieHeader(cookieHeader);

        return (accessToken, refreshToken, tag, authSessionDataValue, oauthTokens);
    }

    private static AuthSession GetAuthSession(string? accessToken, string? refreshToken, string? tagJson, string? authSessionDataValue, Dictionary<OAuthProvider, OAuthProviderToken> oauthTokens, HttpMessageHandler httpMessageHandler)
    {
        if (accessToken == null)
        {
            return new(httpMessageHandler, oauthProviderTokens: oauthTokens, authSessionData: authSessionDataValue);
        }

        try
        {
            object? tag = null;
            if (!string.IsNullOrEmpty(tagJson))
            {
                try
                {
                    tag = JsonSerializer.Deserialize<object>(tagJson, JsonHelper.DefaultOptions);
                }
                catch
                {
                    // If tag deserialization fails, just leave it null
                }
            }

            var token = new AuthToken(accessToken, refreshToken, tag);
            return new(httpMessageHandler, token, oauthTokens, authSessionData: authSessionDataValue);
        }
        catch (Exception)
        {
            return new(httpMessageHandler, oauthProviderTokens: oauthTokens, authSessionData: authSessionDataValue);
        }
    }

    private static Dictionary<OAuthProvider, OAuthProviderToken> ExtractOAuthProviderTokensFromCookies(IRequestCookieCollection cookies)
    {
        var oauthTokens = new Dictionary<OAuthProvider, OAuthProviderToken>();

        foreach (OAuthProvider provider in Enum.GetValues(typeof(OAuthProvider)))
        {
            var prefix = GetProviderPrefix(provider);
            var accessTokenName = $"{prefix}_access_token";
            var refreshTokenName = $"{prefix}_refresh_token";
            var tagName = $"{prefix}_auth_tag";

            var accessToken = cookies[accessTokenName].NullIfEmpty();
            if (accessToken == null)
            {
                continue;
            }

            var refreshToken = cookies[refreshTokenName].NullIfEmpty();
            var tagJson = cookies[tagName].NullIfEmpty();

            object? tag = null;
            if (!string.IsNullOrEmpty(tagJson))
            {
                try
                {
                    tag = JsonSerializer.Deserialize<object>(tagJson, JsonHelper.DefaultOptions);
                }
                catch
                {
                    // If tag deserialization fails, just leave it null
                }
            }

            var authToken = new AuthToken(accessToken, refreshToken, tag);
            var providerToken = new OAuthProviderToken(provider, authToken);
            oauthTokens[provider] = providerToken;
        }

        return oauthTokens;
    }

    private static Dictionary<OAuthProvider, OAuthProviderToken> ExtractOAuthProviderTokensFromCookieHeader(List<CookieHeaderValue> cookieHeader)
    {
        var oauthTokens = new Dictionary<OAuthProvider, OAuthProviderToken>();

        string? GetCookie(string name)
        {
            var rawValue = cookieHeader
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value.Value;
            return !string.IsNullOrEmpty(rawValue)
                ? WebUtility.UrlDecode(rawValue)
                : null;
        }

        foreach (OAuthProvider provider in Enum.GetValues(typeof(OAuthProvider)))
        {
            var prefix = GetProviderPrefix(provider);
            var accessTokenName = $"{prefix}_access_token";
            var refreshTokenName = $"{prefix}_refresh_token";
            var tagName = $"{prefix}_auth_tag";

            var accessToken = GetCookie(accessTokenName);
            if (accessToken == null)
            {
                continue;
            }

            var refreshToken = GetCookie(refreshTokenName);
            var tagJson = GetCookie(tagName);

            object? tag = null;
            if (!string.IsNullOrEmpty(tagJson))
            {
                try
                {
                    tag = JsonSerializer.Deserialize<object>(tagJson, JsonHelper.DefaultOptions);
                }
                catch
                {
                    // If tag deserialization fails, just leave it null
                }
            }

            var authToken = new AuthToken(accessToken, refreshToken, tag);
            var providerToken = new OAuthProviderToken(provider, authToken);
            oauthTokens[provider] = providerToken;
        }

        return oauthTokens;
    }

    private static string GetProviderPrefix(OAuthProvider provider)
    {
        return provider switch
        {
            OAuthProvider.Google => "go",
            OAuthProvider.GitHub => "gh",
            OAuthProvider.Microsoft => "ms",
            OAuthProvider.Apple => "ap",
            OAuthProvider.Twitter => "tw",
            OAuthProvider.Discord => "dc",
            OAuthProvider.Twitch => "tc",
            OAuthProvider.Figma => "fg",
            OAuthProvider.Notion => "nt",
            OAuthProvider.Azure => "az",
            OAuthProvider.WorkOS => "wo",
            OAuthProvider.GitLab => "gl",
            OAuthProvider.Bitbucket => "bb",
            _ => provider.ToString().ToLowerInvariant().Substring(0, Math.Min(2, provider.ToString().Length))
        };
    }
}
