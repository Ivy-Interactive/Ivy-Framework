using Ivy.Auth;
using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Client;

public class ToasterMessage
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class ErrorMessage
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? StackTrace { get; set; }
}

public class HistoryState
{
    public string? TabId { get; set; }
}

public class RedirectMessage
{
    public string? Url { get; set; }
    public bool ReplaceHistory { get; set; }
    public HistoryState? State { get; set; }
}

public class SetAuthTokenMessage
{
    public required AuthToken? AuthToken { get; set; }
    public required bool ReloadPage { get; set; }
}

public class SetRootAppIdMessage
{
    public required string RootAppId { get; set; }
}

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

    public static void Redirect(this IClientProvider client, string url, bool replaceHistory = false, string? tabId = null)
    {
        client.Sender.Send(
            "Redirect",
            new RedirectMessage
            {
                Url = url,
                ReplaceHistory = replaceHistory,
                State = new HistoryState { TabId = tabId }
            });
    }

    public static void SetAuthToken(this IClientProvider client, AuthToken? authToken, bool reloadPage = true)
    {
        client.Sender.Send("SetAuthToken", new SetAuthTokenMessage { AuthToken = authToken, ReloadPage = reloadPage });
    }

    public static void SetRootAppId(this IClientProvider client, string rootAppId)
    {
        client.Sender.Send("SetRootAppId", new SetRootAppIdMessage { RootAppId = rootAppId });
    }

    /// <summary>
    /// Sets the theme mode for the client application (Light, Dark, or System).
    /// </summary>
    /// <param name="client">The client provider instance.</param>
    /// <param name="themeMode">The theme mode to apply (Light, Dark, or System).</param>
    public static void SetThemeMode(this IClientProvider client, ThemeMode themeMode)
    {
        client.Sender.Send("SetTheme", themeMode.ToString());
    }

    /// <summary>
    /// Applies custom theme CSS to the client application.
    /// This method injects the provided CSS directly into the page to override default theme styles.
    /// </summary>
    /// <param name="client">The client provider instance.</param>
    /// <param name="css">The CSS content to inject, typically containing CSS custom properties (variables) for theming.</param>
    public static void ApplyTheme(this IClientProvider client, string css)
    {
        client.Sender.Send("ApplyTheme", css);
    }

    public static void Toast(this IClientProvider client, string description, string? title = null)
    {
        client.Sender.Send("Toast", new ToasterMessage { Description = description, Title = title });
    }

    public static void Toast(this IClientProvider client, Exception ex)
    {
        var innerException = Utils.GetInnerMostException(ex);
        client.Sender.Send("Toast", new ToasterMessage { Description = innerException.Message, Title = "Failed" });
    }

    public static void Error(this IClientProvider client, Exception ex)
    {
        var innerException = Utils.GetInnerMostException(ex);
        var notification = new ErrorMessage
        {
            Description = innerException.Message,
            Title = innerException.GetType().Name,
            StackTrace = innerException.StackTrace
        };
        client.Sender.Send("Error", notification);
    }
}