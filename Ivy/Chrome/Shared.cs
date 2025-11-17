using System.Linq;
using System.Text.Json;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Chrome;

public enum ChromeNavigation
{
    Tabs,
    Pages
}

public record ChromeSettings
{
    public object? Header { get; init; }
    public object? Footer { get; init; }
    public string? DefaultAppId { get; init; }
    public string? WallpaperAppId { get; init; }
    public bool PreventTabDuplicates { get; init; }
    public ChromeNavigation Navigation { get; init; }
    public Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>> FooterMenuItemsTransformer { get; init; } = (items, _) => items;

    public static ChromeSettings Default() => new()
    {
        Navigation = ChromeNavigation.Tabs,
        PreventTabDuplicates = false
    };
}

public static class ChromeSettingsExtensions
{
    public static ChromeSettings Header(this ChromeSettings settings, object? header) => settings with { Header = header };
    public static ChromeSettings Footer(this ChromeSettings settings, object? footer) => settings with { Footer = footer };
    public static ChromeSettings DefaultAppId(this ChromeSettings settings, string? defaultAppId) => settings with { DefaultAppId = defaultAppId };
    public static ChromeSettings DefaultApp<T>(this ChromeSettings settings)
    {
        var type = typeof(T);
        var descriptor = AppHelpers.GetApp(type);
        return settings with { DefaultAppId = descriptor.Id };
    }
    public static ChromeSettings WallpaperAppId(this ChromeSettings settings, string? wallpaperAppId) => settings with { WallpaperAppId = wallpaperAppId };
    public static ChromeSettings WallpaperApp<T>(this ChromeSettings settings)
    {
        var type = typeof(T);
        var descriptor = AppHelpers.GetApp(type);
        return settings with { WallpaperAppId = descriptor.Id };
    }
    public static ChromeSettings Navigation(this ChromeSettings settings, ChromeNavigation navigation) => settings with { Navigation = navigation };
    public static ChromeSettings UseTabs(this ChromeSettings settings, bool preventDuplicates = false) => settings with { Navigation = ChromeNavigation.Tabs, PreventTabDuplicates = preventDuplicates };
    public static ChromeSettings UsePages(this ChromeSettings settings) => settings with { Navigation = ChromeNavigation.Pages };
    public static ChromeSettings UseFooterMenuItemsTransformer(this ChromeSettings settings, Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>> transformer) => settings with { FooterMenuItemsTransformer = transformer };
}

[Signal(BroadcastType.Chrome)]
public class NavigateSignal : AbstractSignal<NavigateArgs, Unit> { }

public enum NavigationPurpose
{
    NewDestination,
    HistoryTraversal,
}

public record NavigateArgs(string? AppId, object? AppArgs = null, string? TabId = null, NavigationPurpose Purpose = NavigationPurpose.NewDestination, bool Chrome = true)
{
    public AppHost ToAppHost(string? parentId = null)
    {
        if (this.AppId == null)
        {
            throw new InvalidOperationException("Cannot create AppHost: AppId is null.");
        }

        return new AppHost(this.AppId, this.AppArgs != null ? JsonSerializer.Serialize(this.AppArgs) : null, parentId);
    }

    public string GetUrl(string? parentId = null)
    {
        // Validate AppId to prevent injection attacks
        if (!Utils.IsSafeAppId(this.AppId))
        {
            throw new InvalidOperationException($"Cannot get URL: Invalid AppId '{this.AppId}'. AppId contains unsafe characters.");
        }

        // Use path-based URL for better user experience
        var url = $"/{this.AppId}";

        // Build query parameters if needed
        var queryParams = new List<string>();

        if (parentId != null)
        {
            queryParams.Add($"parentId={parentId}");
        }

        if (this.AppArgs != null)
        {
            var jsonArgs = JsonSerializer.Serialize(this.AppArgs);
            var encodedArgs = System.Web.HttpUtility.UrlEncode(jsonArgs);
            queryParams.Add($"appArgs={encodedArgs}");
        }

        if (!this.Chrome)
        {
            queryParams.Add("chrome=false");
        }

        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        return url;
    }
}

public static class NavigateSignalExtensions
{
    public static INavigator UseNavigation(this IViewContext context)
    {
        var signal = context.CreateSignal<NavigateSignal, NavigateArgs, Unit>();
        var repository = context.UseService<IAppRepository>();
        var client = context.UseService<IClientProvider>();
        return new Navigator(signal, repository, client);
    }

    public static INavigator UseNavigation(this IView view)
    {
        return view.Context.UseNavigation();
    }

    private class Navigator(ISignalSender<NavigateArgs, Unit> signal, IAppRepository repository, IClientProvider client) : INavigator
    {
        public void Navigate(Type type, object? appArgs = null)
        {
            var appId = repository.GetApp(type)?.Id ??
                        throw new InvalidOperationException($"App '{type.FullName}' not found.");
            signal.Send(new NavigateArgs(appId, appArgs));
        }

        public void Navigate(string uri, object? appArgs = null)
        {
            if (uri.StartsWith("http://") || uri.StartsWith("https://"))
            {
                // Validate external URL to prevent open redirect vulnerabilities
                var validatedUrl = Utils.ValidateLinkUrl(uri);
                if (validatedUrl == null)
                {
                    throw new ArgumentException($"Invalid external URL: {uri}", nameof(uri));
                }
                client.OpenUrl(validatedUrl);
            }
            else if (uri.StartsWith("app://"))
            {
                var appId = uri[6..];
                // Validate AppId to prevent injection attacks
                if (!Utils.IsSafeAppId(appId))
                {
                    throw new ArgumentException($"Invalid AppId: {appId}", nameof(uri));
                }
                signal.Send(new NavigateArgs(appId, appArgs));
            }
        }
    }
}

public interface INavigator
{
    public void Navigate(Type type, object? appArgs = null);
    public void Navigate(string uri, object? appArgs = null);
    public void Navigate<T>(object? appArgs = null)
    {
        Navigate(typeof(T), appArgs);
    }
}
