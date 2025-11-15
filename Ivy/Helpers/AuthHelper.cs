using System.Net;
using System.Text.Json;
using Grpc.Core;
using Ivy.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Core;
using Ivy.Client;

namespace Ivy.Helpers;

/// <summary>Helper class for auth token extraction and validation.</summary>
public static class AuthHelper
{
    /// <summary>
    /// Extracts the auth token from an HttpContext's cookies.
    /// </summary>
    /// <param name="context">The HttpContext containing the request cookies.</param>
    /// <returns>
    /// An AuthToken if valid cookies are found, otherwise null.
    /// Combines the main auth_token cookie with the optional auth_ext_refresh_token cookie.
    /// </returns>
    public static AuthToken? GetAuthToken(HttpContext context)
        => GetAuthCookies(context) is (var authTokenValue, var extRefreshTokenValue)
            ? GetAuthToken(authTokenValue, extRefreshTokenValue)
            : null;

    /// <summary>
    /// Extracts the auth token from a gRPC ServerCallContext's request headers.
    /// </summary>
    /// <param name="context">The ServerCallContext containing the gRPC request headers.</param>
    /// <returns>
    /// An AuthToken if valid cookies are found, otherwise null.
    /// Combines the main auth_token cookie with the optional auth_ext_refresh_token cookie.
    /// </returns>
    public static AuthToken? GetAuthToken(ServerCallContext context)
        => GetAuthCookies(context) is (var authTokenValue, var extRefreshTokenValue)
            ? GetAuthToken(authTokenValue, extRefreshTokenValue)
            : null;

    /// <summary>
    /// Validates authentication for a gRPC request if the server requires it.
    /// Checks if the server has an AuthProviderType configured, and if so, validates the auth token.
    /// </summary>
    /// <param name="server">The Server instance to check for authentication requirements.</param>
    /// <param name="sessionStore">The AppSessionStore to retrieve the session and service provider from.</param>
    /// <param name="connectionId">The connection ID to identify the session.</param>
    /// <param name="context">The gRPC ServerCallContext containing the auth token in cookies.</param>
    /// <exception cref="RpcException">
    /// Thrown with StatusCode.InvalidArgument if connectionId is null or empty.
    /// Thrown with StatusCode.NotFound if the session for the connectionId is not found.
    /// Thrown with StatusCode.Unauthenticated if:
    /// - Authentication is required but no valid token is provided
    /// - The provided token is invalid or expired
    /// Thrown with StatusCode.Internal if:
    /// - The auth provider is not configured when it should be
    /// - An unexpected error occurs during token validation
    /// </exception>
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

        var authToken = GetAuthToken(context);

        var serviceProvider = session.AppServices;
        var clientProvider = serviceProvider.GetRequiredService<IClientProvider>();
        try
        {
            await ValidateAuth(serviceProvider, authToken, context.CancellationToken);
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

    /// <summary>
    /// Validates authentication for an HTTP Controller request if the server requires it.
    /// Checks if the server has an AuthProviderType configured, and if so, validates the auth token from the controller's HttpContext.
    /// </summary>
    /// <param name="controller">The Controller instance to extract the HttpContext from.</param>
    /// <param name="server">The Server instance to check for authentication requirements.</param>
    /// <param name="serviceProvider">The service provider to resolve the IAuthProvider from.</param>
    /// <returns>
    /// An IActionResult representing an error response if authentication fails:
    /// - Unauthorized (401) for missing or invalid tokens
    /// - InternalServerError (500) for configuration or validation errors
    /// Returns null if authentication is not required or if validation succeeds.
    /// </returns>
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
            var authToken = GetAuthToken(controller.HttpContext);
            await ValidateAuth(serviceProvider, authToken, controller.HttpContext.RequestAborted);
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

    private static async Task ValidateAuth(IServiceProvider serviceProvider, AuthToken? authToken, CancellationToken cancellationToken)
    {
        if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
        {
            throw new MissingAuthTokenException();
        }

        // Get auth provider and validate token
        var authProvider = serviceProvider.GetService<IAuthProvider>()
            ?? throw new AuthProviderNotConfiguredException();

        try
        {
            var isValid = await authProvider.ValidateAccessTokenAsync(authToken.AccessToken, cancellationToken);
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

    private static (string AuthToken, string? ExtRefreshToken)? GetAuthCookies(HttpContext context)
    {
        var cookies = context.Request.Cookies;
        var authTokenValue = cookies["auth_token"].NullIfEmpty();
        if (authTokenValue == null)
        {
            return null;
        }

        var extRefreshTokenValue = cookies["auth_ext_refresh_token"].NullIfEmpty();
        return (authTokenValue, extRefreshTokenValue);
    }

    private static (string AuthToken, string? ExtRefreshToken)? GetAuthCookies(ServerCallContext context)
    {
        var cookies = context.RequestHeaders.GetValue("cookie") ?? string.Empty;
        if (string.IsNullOrEmpty(cookies))
        {
            return null;
        }

        var cookieHeader = CookieHeaderValue.ParseList([cookies]).ToList();
        var rawAuthTokenValue = cookieHeader
            .FirstOrDefault(c => c.Name.Equals("auth_token", StringComparison.OrdinalIgnoreCase))?.Value.Value;

        if (string.IsNullOrEmpty(rawAuthTokenValue))
        {
            return null;
        }

        var authTokenValue = WebUtility.UrlDecode(rawAuthTokenValue);

        var rawExtRefreshTokenValue = cookieHeader
            .FirstOrDefault(c => c.Name.Equals("auth_ext_refresh_token", StringComparison.OrdinalIgnoreCase))?.Value.Value;

        var extRefreshTokenValue = !string.IsNullOrEmpty(rawExtRefreshTokenValue)
            ? WebUtility.UrlDecode(rawExtRefreshTokenValue)
            : null;

        return (authTokenValue, extRefreshTokenValue);
    }

    private static AuthToken? GetAuthToken(string authTokenValue, string? extRefreshTokenValue)
    {
        try
        {
            var token = JsonSerializer.Deserialize<AuthToken>(authTokenValue);
            if (token == null)
            {
                return null;
            }

            // Check if refresh token is in a separate cookie
            if (token.RefreshToken == null)
            {
                return token with { RefreshToken = extRefreshTokenValue };
            }

            return token;
        }
        catch (Exception)
        {
            return null;
        }
    }
}