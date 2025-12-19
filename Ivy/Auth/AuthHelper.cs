using System.Net;
using System.Text.Json;
using Grpc.Core;
using Ivy.Auth;
using Ivy.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Core;
using Ivy.Client;

namespace Ivy.Helpers;

public static class AuthHelper
{
    public static AuthSession GetAuthSession(HttpContext context, HttpMessageHandler httpMessageHandler)
    => GetAuthCookies(context) is (var authToken, var extRefreshToken, var authSessionData)
        ? GetAuthSession(authToken, extRefreshToken, authSessionData, httpMessageHandler)
        : new AuthSession(httpMessageHandler);

    public static AuthSession GetAuthSession(ServerCallContext context, HttpMessageHandler httpMessageHandler)
    => GetAuthCookies(context) is (var authToken, var extRefreshToken, var authSessionData)
        ? GetAuthSession(authToken, extRefreshToken, authSessionData, httpMessageHandler)
        : new AuthSession(httpMessageHandler);

    public static async Task ValidateAuthIfRequired(Server server, AppSessionStore sessionStore, string connectionId, ServerCallContext context)
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

    public static async Task<IActionResult?> ValidateAuthIfRequired(this Controller controller, Server server, IServiceProvider serviceProvider)
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

    private static (string? AuthToken, string? ExtRefreshToken, string? AuthSessionData) GetAuthCookies(HttpContext context)
    {
        var cookies = context.Request.Cookies;
        var authTokenValue = cookies["auth_token"].NullIfEmpty();
        var extRefreshTokenValue = cookies["auth_ext_refresh_token"].NullIfEmpty();
        var authSessionDataValue = cookies["auth_session_data"].NullIfEmpty();
        return (authTokenValue, extRefreshTokenValue, authSessionDataValue);
    }

    private static (string? AuthToken, string? ExtRefreshToken, string? AuthSessionData) GetAuthCookies(ServerCallContext context)
    {
        var cookies = context.RequestHeaders.GetValue("cookie") ?? string.Empty;
        if (string.IsNullOrEmpty(cookies))
        {
            return (null, null, null);
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

        var authTokenValue = GetCookie("auth_token");
        var extRefreshTokenValue = GetCookie("auth_ext_refresh_token");
        var authSessionDataValue = GetCookie("auth_session_data");

        return (authTokenValue, extRefreshTokenValue, authSessionDataValue);
    }

    private static AuthSession GetAuthSession(string? authTokenValue, string? extRefreshTokenValue, string? authSessionDataValue, HttpMessageHandler httpMessageHandler)
    {
        if (authTokenValue == null)
        {
            return new(httpMessageHandler, authSessionData: authSessionDataValue);
        }

        try
        {
            var token = JsonSerializer.Deserialize<AuthToken>(authTokenValue, JsonHelper.DefaultOptions);
            if (token == null)
            {
                return new(httpMessageHandler, authSessionData: authSessionDataValue);
            }

            // Check if refresh token is in a separate cookie
            if (token.RefreshToken == null)
            {
                token = token with { RefreshToken = extRefreshTokenValue };
            }

            return new(httpMessageHandler, token, authSessionDataValue);
        }
        catch (Exception)
        {
            return new(httpMessageHandler, authSessionData: authSessionDataValue);
        }
    }
}