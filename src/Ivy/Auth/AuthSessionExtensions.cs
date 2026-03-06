using System.Text.Json;
using Ivy.Core.Auth;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AuthSessionExtensions
{
#if DEBUG
    internal static CheckedAuthSessionBuilder WithCheckedAccess(this IAuthProviderSession authSession)
        => new(authSession);
#endif

    public static AuthProviderSessionSnapshot TakeSnapshot(this IAuthProviderSession authSession)
        => new()
        {
            AuthToken = authSession.AuthToken,
            OAuthProviderSessions = new Dictionary<string, IAuthTokenHandlerSession>(authSession.OAuthProviderSessions),
            AuthSessionData = authSession.AuthSessionData,
        };

    public static bool HasChangedSince(this IAuthProviderSession authSession, AuthProviderSessionSnapshot snapshot)
        => authSession.AuthToken != snapshot.AuthToken ||
           authSession.AuthSessionData != snapshot.AuthSessionData ||
           !OAuthProviderSessionsEqual(authSession.OAuthProviderSessions, snapshot.OAuthProviderSessions);

    private static bool OAuthProviderSessionsEqual(
        IReadOnlyDictionary<string, IAuthTokenHandlerSession> current,
        IReadOnlyDictionary<string, IAuthTokenHandlerSession> snapshot)
    {
        if (current.Count != snapshot.Count) return false;

        foreach (var kvp in current)
        {
            if (!snapshot.TryGetValue(kvp.Key, out var snapshotSession))
            {
                return false;
            }

            // Compare the sessions by their auth tokens
            if (kvp.Value.AuthToken != snapshotSession.AuthToken ||
                kvp.Value.AuthSessionData != snapshotSession.AuthSessionData)
            {
                return false;
            }
        }

        return true;
    }

    public static T? GetAuthSessionData<T>(this IAuthTokenHandlerSession authSession)
    {
        if (string.IsNullOrEmpty(authSession.AuthSessionData))
        {
            return default;
        }

        return typeof(T) == typeof(string)
            ? (T)(object)authSession.AuthSessionData
            : JsonSerializer.Deserialize<T>(authSession.AuthSessionData, JsonHelper.DefaultOptions);
    }

    public static void SetAuthSessionData<T>(this IAuthTokenHandlerSession authSession, T? data)
    {
        if (data == null)
        {
            authSession.AuthSessionData = null;
        }
        else if (data is string strData)
        {
            authSession.AuthSessionData = strData;
        }
        else
        {
            authSession.AuthSessionData = JsonSerializer.Serialize(data, JsonHelper.DefaultOptions);
        }
    }
}
