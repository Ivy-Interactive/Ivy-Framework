using System.Net;
using System.Text.Json;
using Grpc.Core;
using Ivy.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Helpers;

/// <summary>
/// Helper class for auth token extraction and validation.
/// </summary>
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
    /// <param name="serviceProvider">The service provider to resolve the IAuthProvider from.</param>
    /// <param name="context">The gRPC ServerCallContext containing the auth token in headers.</param>
    /// <exception cref="RpcException">
    /// Thrown with StatusCode.Unauthenticated if:
    /// - Authentication is required but no valid token is provided
    /// - The provided token is invalid or expired
    /// Thrown with StatusCode.Internal if:
    /// - The auth provider is not configured when it should be
    /// - An unexpected error occurs during token validation
    /// </exception>
    /// <remarks>
    /// This method is a no-op if the server does not require authentication (server.AuthProviderType == null).
    /// </remarks>
    public static async Task ValidateAuthIfRequired(Server server, IServiceProvider serviceProvider, ServerCallContext context)
    {
        // Check if auth is required
        if (server.AuthProviderType == null)
        {
            return;
        }

        var authToken = GetAuthToken(context);

        if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication required."));
        }

        // Get auth provider and validate token
        var authProvider = serviceProvider.GetService<IAuthProvider>()
            ?? throw new RpcException(new Status(StatusCode.Internal, "Auth provider not configured."));

        try
        {
            var isValid = await authProvider.ValidateAccessTokenAsync(authToken.AccessToken, context.CancellationToken);
            if (!isValid)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired auth token."));
            }
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Error validating auth token."));
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