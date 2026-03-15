using Ivy.Core;
using Ivy.Core.Auth;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ToastVariant
{
    Default,
    Destructive,
    Success,
    Warning,
    Info
}

public static class ClientExtensions
{

    public class ToasterMessage
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ToastVariant Variant { get; set; } = ToastVariant.Default;

        public ToasterMessage Default() { Variant = ToastVariant.Default; return this; }
        public ToasterMessage Destructive() { Variant = ToastVariant.Destructive; return this; }
        public ToasterMessage Success() { Variant = ToastVariant.Success; return this; }
        public ToasterMessage Warning() { Variant = ToastVariant.Warning; return this; }
        public ToasterMessage Info() { Variant = ToastVariant.Info; return this; }
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

    public class SetAuthCookiesMessage
    {
        public required string CookieJarId { get; set; }
        public required bool ReloadPage { get; set; }
        public required bool TriggerMachineReload { get; set; }
    }

    public class SetRootAppIdMessage
    {
        public required string RootAppId { get; set; }
    }

    public static void CopyToClipboard(this IClientProvider client, string content)
    {
        client.Sender.Send("CopyToClipboard", content);
    }

    public static void OpenUrl(this IClientProvider client, string url)
    {
        // Validate URL to prevent open redirect vulnerabilities
        var validatedUrl = ValidationHelper.ValidateLinkUrl(url);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));
        }
        client.Sender.Send("OpenUrl", validatedUrl);
    }
    public static void OpenUrl(this IClientProvider client, Uri uri)
    {
        // Validate URL to prevent open redirect vulnerabilities
        var validatedUrl = ValidationHelper.ValidateLinkUrl(uri.ToString());
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {uri}", nameof(uri));
        }
        client.Sender.Send("OpenUrl", validatedUrl);
    }

    public static void Redirect(this IClientProvider client, string url, bool replaceHistory = false, string? tabId = null)
    {
        // Validate URL to prevent open redirect vulnerabilities
        // For redirects, only allow relative paths or same-origin URLs (frontend will enforce same-origin)
        var validatedUrl = ValidationHelper.ValidateRedirectUrl(url, allowExternal: false);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid redirect URL: {url}. Only relative paths or same-origin URLs are allowed.", nameof(url));
        }
        client.Sender.Send(
            "Redirect",
            new RedirectMessage
            {
                Url = validatedUrl,
                ReplaceHistory = replaceHistory,
                State = new HistoryState { TabId = tabId }
            });
    }

    public static void SetTitle(this IClientProvider client, string? title, string? metaTitle = null)
    {
        var hasTitle = !string.IsNullOrWhiteSpace(title);
        var hasMetaTitle = !string.IsNullOrWhiteSpace(metaTitle);
        if (hasTitle && hasMetaTitle)
        {
            title = $"{title} - {metaTitle}";
        }
        else if (hasMetaTitle)
        {
            title = metaTitle;
        }
        else if (!hasTitle)
        {
            title = "Ivy";
        }
        client.Sender.Send("SetTitle", title);
    }

    public static void SetAuthCookies(this IClientProvider client, CookieJarId cookieJarId, bool reloadPage, bool? triggerMachineReload = null)
    {
        client.Sender.Send(
            "SetAuthCookies",
            new SetAuthCookiesMessage
            {
                CookieJarId = cookieJarId.Value,
                ReloadPage = reloadPage,
                TriggerMachineReload = triggerMachineReload ?? reloadPage
            });
    }

    public static void ReloadPage(this IClientProvider client)
    {
        client.Sender.Send("ReloadPage", new { });
    }

    public static void SetRootAppId(this IClientProvider client, string rootAppId)
    {
        client.Sender.Send("SetRootAppId", new SetRootAppIdMessage { RootAppId = rootAppId });
    }

    public static void SetThemeMode(this IClientProvider client, ThemeMode themeMode)
    {
        client.Sender.Send("SetTheme", themeMode.ToString());
    }

    public static void ApplyTheme(this IClientProvider client, string css)
    {
        client.Sender.Send("ApplyTheme", css);
    }

    public static ToasterMessage Toast(this IClientProvider client, string description, string? title = null, ToastVariant variant = ToastVariant.Default)
    {
        var message = new ToasterMessage { Description = description, Title = title, Variant = variant };
        client.Sender.Send("Toast", message);
        return message;
    }

    public static ToasterMessage Toast(this IClientProvider client, Exception ex, ToastVariant variant = ToastVariant.Default)
    {
        var innerException = ExceptionHelper.GetInnerMostException(ex);
        var message = new ToasterMessage { Description = innerException.Message, Title = "Failed", Variant = variant };
        client.Sender.Send("Toast", message);
        return message;
    }

    public static void Error(this IClientProvider client, Exception ex)
    {
        var innerException = ExceptionHelper.GetInnerMostException(ex);
        var notification = new ErrorMessage
        {
            Description = innerException.Message,
            Title = innerException.GetType().Name,
            StackTrace = innerException.StackTrace
        };
        client.Sender.Send("Error", notification);
    }
}
