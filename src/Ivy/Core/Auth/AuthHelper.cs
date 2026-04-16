using System.Net;
using Grpc.Core;
using Ivy.Core.Server;
using Ivy.Core.HttpTunneling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Auth;

public static class AuthHelper
{
    public static AuthSession GetAuthSession(HttpContext context, TunneledHttpMessageHandler? httpMessageHandler)
    => GetAuthCookies(context) is (var accessToken, var refreshToken, var tag, var authSessionData, var brokeredSessions)
        ? GetAuthSession(accessToken, refreshToken, tag, authSessionData, brokeredSessions, httpMessageHandler)
        : new AuthSession(httpMessageHandler: httpMessageHandler);

    public static AuthSession GetAuthSession(ServerCallContext context, TunneledHttpMessageHandler? httpMessageHandler)
    => GetAuthCookies(context) is (var accessToken, var refreshToken, var tag, var authSessionData, var brokeredSessions)
        ? GetAuthSession(accessToken, refreshToken, tag, authSessionData, brokeredSessions, httpMessageHandler)
        : new AuthSession(httpMessageHandler: httpMessageHandler);

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
        var httpMessageHandler = serviceProvider.GetService<TunneledHttpMessageHandler>();
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
            var httpMessageHandler = serviceProvider.GetService<TunneledHttpMessageHandler>();
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

    private static (string? AccessToken, string? RefreshToken, string? Tag, string? AuthSessionData, Dictionary<string, IAuthTokenHandlerSession> BrokeredSessions) GetAuthCookies(HttpContext context)
    {
        var cookies = context.Request.Cookies;
        var accessToken = cookies[CookieRegistryExtensions.PrefixCookieName("access_token")].NullIfEmpty();
        var refreshToken = cookies[CookieRegistryExtensions.PrefixCookieName("refresh_token")].NullIfEmpty();
        var tag = cookies[CookieRegistryExtensions.PrefixCookieName("auth_tag")].NullIfEmpty();
        var authSessionDataValue = cookies[CookieRegistryExtensions.PrefixCookieName("auth_session_data")].NullIfEmpty();

        var brokeredSessions = ExtractBrokeredSessionsFromCookies(cookies);

        return (accessToken, refreshToken, tag, authSessionDataValue, brokeredSessions);
    }

    private static (string? AccessToken, string? RefreshToken, string? Tag, string? AuthSessionData, Dictionary<string, IAuthTokenHandlerSession> BrokeredSessions) GetAuthCookies(ServerCallContext context)
    {
        var cookies = context.RequestHeaders.GetValue("cookie") ?? string.Empty;
        if (string.IsNullOrEmpty(cookies))
        {
            return (null, null, null, null, new Dictionary<string, IAuthTokenHandlerSession>());
        }

        var cookieHeader = CookieHeaderValue.ParseList([cookies]).ToList();

        string? GetCookie(string name)
        {
            var prefixedName = CookieRegistryExtensions.PrefixCookieName(name);
            var rawValue = cookieHeader
                .FirstOrDefault(c => c.Name.Equals(prefixedName, StringComparison.OrdinalIgnoreCase))?.Value.Value;
            return !string.IsNullOrEmpty(rawValue)
                ? WebUtility.UrlDecode(rawValue)
                : null;
        }

        var accessToken = GetCookie("access_token");
        var refreshToken = GetCookie("refresh_token");
        var tag = GetCookie("auth_tag");
        var authSessionDataValue = GetCookie("auth_session_data");

        var brokeredSessions = ExtractBrokeredSessionsFromCookieHeader(cookieHeader);

        return (accessToken, refreshToken, tag, authSessionDataValue, brokeredSessions);
    }

    private static AuthSession GetAuthSession(string? accessToken, string? refreshToken, string? tag, string? authSessionDataValue, Dictionary<string, IAuthTokenHandlerSession> brokeredSessions, TunneledHttpMessageHandler? httpMessageHandler)
    {
        if (accessToken == null)
        {
            return new(null, authSessionDataValue, httpMessageHandler, brokeredSessions);
        }

        try
        {
            var token = new AuthToken(accessToken, refreshToken, tag);
            return new(token, authSessionDataValue, httpMessageHandler, brokeredSessions);
        }
        catch (Exception)
        {
            return new(null, authSessionDataValue, httpMessageHandler, brokeredSessions);
        }
    }

    internal static Dictionary<string, IAuthTokenHandlerSession> ExtractBrokeredSessionsFromCookies(IRequestCookieCollection cookies)
    {
        var brokeredSessions = new Dictionary<string, IAuthTokenHandlerSession>();

        var primaryAccessToken = CookieRegistryExtensions.PrefixCookieName("access_token");
        var suffix = "_access_token";
        var prefix = primaryAccessToken[..^"access_token".Length];

        var accessTokenCookies = cookies.Keys
            .Where(key => key.EndsWith(suffix)
                && key != primaryAccessToken
                && (prefix.Length == 0 || key.StartsWith(prefix)))
            .ToList();

        foreach (var accessTokenName in accessTokenCookies)
        {
            // Extract provider by removing prefix and suffix
            var cookieName = prefix.Length > 0 ? accessTokenName[prefix.Length..] : accessTokenName;
            var provider = cookieName[..^suffix.Length];

            var accessToken = cookies[accessTokenName].NullIfEmpty();
            if (accessToken == null)
            {
                continue;
            }

            var refreshToken = cookies[CookieRegistryExtensions.PrefixCookieName($"{provider}_refresh_token")].NullIfEmpty();
            var tag = cookies[CookieRegistryExtensions.PrefixCookieName($"{provider}_auth_tag")].NullIfEmpty();

            var authToken = new AuthToken(accessToken, refreshToken, tag);
            var session = new AuthTokenHandlerSession(authToken: authToken);
            brokeredSessions[provider] = session;
        }

        return brokeredSessions;
    }

    internal static Dictionary<string, IAuthTokenHandlerSession> ExtractBrokeredSessionsFromCookieHeader(List<CookieHeaderValue> cookieHeader)
    {
        var brokeredSessions = new Dictionary<string, IAuthTokenHandlerSession>();

        string? GetCookie(string name)
        {
            var rawValue = cookieHeader
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value.Value;
            return !string.IsNullOrEmpty(rawValue)
                ? WebUtility.UrlDecode(rawValue)
                : null;
        }

        var primaryAccessToken = CookieRegistryExtensions.PrefixCookieName("access_token");
        var suffix = "_access_token";
        var prefix = primaryAccessToken[..^"access_token".Length];

        var accessTokenCookies = cookieHeader
            .Where(c => c.Name.Value != null
                && c.Name.Value.EndsWith(suffix)
                && c.Name.Value != primaryAccessToken
                && (prefix.Length == 0 || c.Name.Value.StartsWith(prefix)))
            .Select(c => c.Name.Value!)
            .ToList();

        foreach (var accessTokenName in accessTokenCookies)
        {
            // Extract provider by removing prefix and suffix
            var cookieName = prefix.Length > 0 ? accessTokenName[prefix.Length..] : accessTokenName;
            var provider = cookieName[..^suffix.Length];

            var accessToken = GetCookie(accessTokenName);
            if (accessToken == null)
            {
                continue;
            }

            var refreshToken = GetCookie(CookieRegistryExtensions.PrefixCookieName($"{provider}_refresh_token"));
            var tag = GetCookie(CookieRegistryExtensions.PrefixCookieName($"{provider}_auth_tag"));

            var authToken = new AuthToken(accessToken, refreshToken, tag);
            var session = new AuthTokenHandlerSession(authToken: authToken);
            brokeredSessions[provider] = session;
        }

        return brokeredSessions;
    }
}
