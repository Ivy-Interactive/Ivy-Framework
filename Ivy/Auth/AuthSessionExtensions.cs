using System.Text.Json;
using Ivy.Core.Helpers;

namespace Ivy.Auth;

public static class AuthSessionExtensions
{
#if DEBUG
    internal static CheckedAuthSessionBuilder WithCheckedAccess(this IAuthSession authSession)
        => new(authSession);
#endif

    public static AuthSessionSnapshot TakeSnapshot(this IAuthSession authSession)
        => new()
        {
            AuthToken = authSession.AuthToken,
            AuthSessionData = authSession.AuthSessionData,
        };

    public static bool HasChangedSince(this IAuthSession authSession, AuthSessionSnapshot snapshot)
        => authSession.AuthToken != snapshot.AuthToken ||
           authSession.AuthSessionData != snapshot.AuthSessionData;

    public static T? GetAuthSessionData<T>(this IAuthSession authSession)
    {
        if (string.IsNullOrEmpty(authSession.AuthSessionData))
        {
            return default;
        }

        return typeof(T) == typeof(string)
            ? (T)(object)authSession.AuthSessionData
            : JsonSerializer.Deserialize<T>(authSession.AuthSessionData, JsonHelper.DefaultOptions);
    }

    public static void SetAuthSessionData<T>(this IAuthSession authSession, T? data)
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
