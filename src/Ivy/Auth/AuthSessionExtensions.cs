using System.Text.Json;
using Ivy.Core.Auth;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AuthSessionExtensions
{
#if DEBUG
    internal static CheckedAuthTokenHandlerSessionBuilder WithCheckedAccess(this IAuthTokenHandlerSession authSession)
        => new(authSession);

    internal static CheckedAuthSessionBuilder WithCheckedAccess(this IAuthSession authSession)
        => new(authSession);
#endif

    public static AuthTokenHandlerSessionSnapshot TakeSnapshot(this IAuthTokenHandlerSession authSession)
        => new()
        {
            AuthToken = authSession.AuthToken,
            AuthSessionData = authSession.AuthSessionData,
        };

    public static bool HasChangedSince(this IAuthTokenHandlerSession authSession, AuthTokenHandlerSessionSnapshot snapshot)
        => authSession.AuthToken != snapshot.AuthToken ||
           authSession.AuthSessionData != snapshot.AuthSessionData;

    public static AuthSessionSnapshot TakeSnapshot(this IAuthSession authSession)
        => new()
        {
            AuthToken = authSession.AuthToken,
            BrokeredSessions = new Dictionary<string, IAuthTokenHandlerSession>(authSession.BrokeredSessions),
            AuthSessionData = authSession.AuthSessionData,
        };

    public static bool HasChangedSince(this IAuthSession authSession, AuthSessionSnapshot snapshot)
        => authSession.AuthToken != snapshot.AuthToken ||
           authSession.AuthSessionData != snapshot.AuthSessionData ||
           !BrokeredSessionsEqual(authSession.BrokeredSessions, snapshot.BrokeredSessions);

    private static bool BrokeredSessionsEqual(
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

    public static T? GetAuthSessionData<T>(this IAuthTokenHandlerSession authSession) where T : class
    {
        if (string.IsNullOrEmpty(authSession.AuthSessionData))
        {
            return null;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)authSession.AuthSessionData;
        }
        else
        {
            try
            {
                return JsonSerializer.Deserialize<T>(authSession.AuthSessionData, JsonHelper.IgnoreNullOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public static void SetAuthSessionData<T>(this IAuthTokenHandlerSession authSession, T? data) where T : class
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
            authSession.AuthSessionData = JsonSerializer.Serialize(data, JsonHelper.IgnoreNullOptions);
        }
    }
}
