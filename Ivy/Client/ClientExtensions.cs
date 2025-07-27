using Ivy.Auth;
using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Client;

public static class ClientExtensions
{
    public static void CopyToClipboard(this IClientProvider client, string content)
    {
        client.Sender.Send("CopyToClipboard", content);
    }

    public static void OpenUrl(this IClientProvider client, string url)
    {
        client.Sender.Send("OpenUrl", url);
    }
    public static void OpenUrl(this IClientProvider client, Uri uri)
    {
        client.Sender.Send("OpenUrl", uri.ToString());
    }

    public static void Redirect(this IClientProvider client, string url)
    {
        client.Sender.Send("Redirect", url);
    }

    public static void SetJwt(this IClientProvider client, AuthToken? authToken)
    {
        client.Sender.Send("SetJwt", authToken);
    }

    public static void SetTheme(this IClientProvider client, Theme theme)
    {
        client.Sender.Send("SetTheme", theme.ToString());
    }
}