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