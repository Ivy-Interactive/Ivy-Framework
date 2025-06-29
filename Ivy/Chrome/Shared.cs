using System.Text.Json;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;

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
    public ChromeNavigation Navigation { get; init; }

    public static ChromeSettings Default() => new()
    {
        Navigation = ChromeNavigation.Tabs
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
    public static ChromeSettings Navigation(this ChromeSettings settings, ChromeNavigation navigation) => settings with { Navigation = navigation };
    public static ChromeSettings UseTabs(this ChromeSettings settings) => settings with { Navigation = ChromeNavigation.Tabs };
    public static ChromeSettings UsePages(this ChromeSettings settings) => settings with { Navigation = ChromeNavigation.Pages };
}

[Signal(BroadcastType.Chrome)]
public class NavigateSignal : AbstractSignal<NavigateArgs, Unit> { }

public record NavigateArgs(string AppId, object? AppArgs = null)
{
    public string GetUrl(string? parentId = null)
    {
        var url = $"app.html?appId={this.AppId}";
        if (parentId != null)
        {
            url += $"&parentId={parentId}";
        }
        if (this.AppArgs != null)
        {
            var jsonArgs = JsonSerializer.Serialize(this.AppArgs);
            var encodedArgs = System.Web.HttpUtility.UrlEncode(jsonArgs);
            url += $"&appArgs={encodedArgs}";
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
                client.OpenUrl(uri);
            }
            else if (uri.StartsWith("app://"))
            {
                var appId = uri[6..];
                signal.Send(new NavigateArgs(appId, appArgs));
            }
        }
    }
}

public interface INavigator
{
    public void Navigate(Type type, object? appArgs = null);
    public void Navigate(string uri, object? appArgs = null);
}


