using System.Collections.Generic;
using System.Runtime.Serialization;
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
        [DataMember(Name = "title")]
        public string? Title { get; set; }
        [DataMember(Name = "description")]
        public string? Description { get; set; }

        [DataMember(Name = "variant")]
        public string VariantString => Variant.ToString().ToLower();

        [IgnoreDataMember]
        public ToastVariant Variant { get; set; } = ToastVariant.Default;

        public ToasterMessage Default() { Variant = ToastVariant.Default; return this; }
        public ToasterMessage Destructive() { Variant = ToastVariant.Destructive; return this; }
        public ToasterMessage Success() { Variant = ToastVariant.Success; return this; }
        public ToasterMessage Warning() { Variant = ToastVariant.Warning; return this; }
        public ToasterMessage Info() { Variant = ToastVariant.Info; return this; }
    }

    public class ErrorMessage
    {
        [DataMember(Name = "title")]
        public required string Title { get; set; }
        [DataMember(Name = "description")]
        public required string Description { get; set; }
        [DataMember(Name = "stackTrace")]
        public string? StackTrace { get; set; }
    }

    public class HistoryState
    {
        [DataMember(Name = "tabId")]
        public string? TabId { get; set; }
    }

    public class RedirectMessage
    {
        [DataMember(Name = "url")]
        public string? Url { get; set; }
        [DataMember(Name = "replaceHistory")]
        public bool ReplaceHistory { get; set; }
        [DataMember(Name = "state")]
        public HistoryState? State { get; set; }
    }

    public class SetAuthCookiesMessage
    {
        [DataMember(Name = "cookieJarId")]
        public required string CookieJarId { get; set; }
        [DataMember(Name = "reloadPage")]
        public required bool ReloadPage { get; set; }
        [DataMember(Name = "triggerMachineReload")]
        public required bool TriggerMachineReload { get; set; }
        [DataMember(Name = "triggerMachineBrokeredRefresh")]
        public required bool TriggerMachineBrokeredRefresh { get; set; }
    }

    public class SetRootAppIdMessage
    {
        [DataMember(Name = "rootAppId")]
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
            new Dictionary<string, object?>
            {
                ["url"] = validatedUrl,
                ["replaceHistory"] = replaceHistory,
                ["state"] = new Dictionary<string, object?> { ["tabId"] = tabId }
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

    public static void SetAuthCookies(this IClientProvider client, CookieJarId cookieJarId, bool reloadPage, bool? triggerMachineReload = null, bool triggerMachineBrokeredRefresh = false)
    {
        client.Sender.Send(
            "SetAuthCookies",
            new Dictionary<string, object?>
            {
                ["cookieJarId"] = cookieJarId.Value,
                ["reloadPage"] = reloadPage,
                ["triggerMachineReload"] = triggerMachineReload ?? reloadPage,
                ["triggerMachineBrokeredRefresh"] = triggerMachineBrokeredRefresh
            });
    }

    public static void ReloadPage(this IClientProvider client)
    {
        client.Sender.Send("ReloadPage", new Dictionary<string, object?>());
    }

    public static void RefreshAuthFromCookies(this IClientProvider client)
    {
        client.Sender.Send("RefreshAuthFromCookies", new Dictionary<string, object?>());
    }

    public static void SetRootAppId(this IClientProvider client, string rootAppId)
    {
        client.Sender.Send("SetRootAppId", new Dictionary<string, object?> { ["rootAppId"] = rootAppId });
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
        client.Sender.Send("Toast", new Dictionary<string, object?> { ["title"] = message.Title, ["description"] = message.Description, ["variant"] = message.Variant.ToString().ToLower() });
        return message;
    }

    public static ToasterMessage Toast(this IClientProvider client, Exception ex, ToastVariant variant = ToastVariant.Default)
    {
        var innerException = ExceptionHelper.GetInnerMostException(ex);
        var message = new ToasterMessage { Description = innerException.Message, Title = "Failed", Variant = variant };
        client.Sender.Send("Toast", new Dictionary<string, object?> { ["title"] = message.Title, ["description"] = message.Description, ["variant"] = message.Variant.ToString().ToLower() });
        return message;
    }

    public static void Error(this IClientProvider client, Exception ex)
    {
        var innerException = ExceptionHelper.GetInnerMostException(ex);
        client.Sender.Send("Error", new Dictionary<string, object?>
        {
            ["description"] = innerException.Message,
            ["title"] = innerException.GetType().Name,
            ["stackTrace"] = innerException.StackTrace
        });
    }
}
