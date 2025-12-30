using System.Net;
using Ivy.Chrome;
using Microsoft.AspNetCore.Http;

namespace Ivy.Apps;

public record AppRouteResult(
    string AppId,
    string? NavigationAppId,
    AppDescriptor AppDescriptor,
    IAppRepository AppRepository,
    bool ShowChrome,
    int? HttpStatusCode
);

public class AppRouter(Server server)
{
    public AppRouteResult Resolve(HttpContext httpContext)
    {
        var chrome = GetChrome(httpContext);
        var parentId = GetParentId(httpContext);
        var (appId, navigationAppId) = GetAppId(httpContext, chrome);

        return Resolve(appId, navigationAppId, parentId, chrome);
    }

    private AppRouteResult Resolve(string? appId, string? navigationAppId, string? parentId, bool chrome)
    {
        if (string.IsNullOrEmpty(appId))
        {
            return ResolveDefaultApp(navigationAppId, parentId, chrome);
        }

        return ResolveExplicitApp(appId, chrome);
    }

    private static bool GetChrome(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue("chrome", out var chromeParam))
        {
            return !chromeParam.ToString().Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public static string? GetParentId(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue("parentId", out var parentIdParam))
        {
            var value = parentIdParam.ToString();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        return null;
    }

    private static (string? AppId, string? NavigationAppId) GetAppId(HttpContext httpContext, bool chrome)
    {
        string? appId = null;
        string? navigationAppId = null;

        if (httpContext.Request.Query.TryGetValue("appId", out var appIdParam))
        {
            var id = appIdParam.ToString();
            if (string.IsNullOrEmpty(id) || id == AppIds.Chrome || id == AppIds.Auth || id == AppIds.Default)
            {
                id = null;
            }

            if (chrome)
            {
                navigationAppId = id;
            }
            else
            {
                appId = id;
            }
        }

        return (appId, navigationAppId);
    }

    public static string GetMachineId(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue("machineId", out var machineIdParam))
        {
            var value = machineIdParam.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException("Missing machineId in request.");
    }

    private AppRouteResult ResolveDefaultApp(string? navigationAppId, string? parentId, bool chrome)
    {
        var appId = server.DefaultAppId ?? server.AppRepository.GetAppOrDefault(null).Id;
        var chromeApp = server.AppRepository.GetAppOrDefault(AppIds.Chrome);

        string? resolvedNavigationAppId = navigationAppId;

        if (chromeApp?.Id == AppIds.Chrome)
        {
            string? chromeDefaultAppId = GetChromeDefaultAppId(chromeApp);

            if (appId == AppIds.Chrome && (parentId != null || !chrome))
            {
                appId = chromeDefaultAppId;
            }
            else if (chrome && navigationAppId == null)
            {
                resolvedNavigationAppId = chromeDefaultAppId;
            }
        }

        if (!string.IsNullOrEmpty(resolvedNavigationAppId))
        {
            return ResolveNavigationApp(appId, resolvedNavigationAppId, chromeApp, chrome);
        }

        var appDescriptor = server.GetApp(appId ?? AppIds.Default);

        return new AppRouteResult(
            appId ?? AppIds.Default,
            resolvedNavigationAppId,
            appDescriptor,
            server.AppRepository,
            chrome,
            null
        );
    }

    private AppRouteResult ResolveNavigationApp(string? appId, string navigationAppId, AppDescriptor? chromeApp, bool chrome)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(navigationAppId);

        if (resolvedApp.Id != navigationAppId)
        {
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                var scopedRepository = new ScopedAppRepository(server.AppRepository, navigationAppId, notFoundApp);

                if (chromeApp?.Id != AppIds.Chrome)
                {
                    return new AppRouteResult(
                        appId ?? AppIds.Default,
                        navigationAppId,
                        notFoundApp,
                        scopedRepository,
                        chrome,
                        (int)HttpStatusCode.NotFound
                    );
                }

                var appDescriptor = server.GetApp(appId ?? AppIds.Default);
                return new AppRouteResult(
                    appId ?? AppIds.Default,
                    navigationAppId,
                    appDescriptor,
                    scopedRepository,
                    chrome,
                    (int)HttpStatusCode.NotFound
                );
            }
        }

        var descriptor = server.GetApp(appId ?? AppIds.Default);
        return new AppRouteResult(
            appId ?? AppIds.Default,
            navigationAppId,
            descriptor,
            server.AppRepository,
            chrome,
            null
        );
    }

    private AppRouteResult ResolveExplicitApp(string appId, bool chrome)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(appId);

        if (resolvedApp.Id != appId)
        {
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                var scopedRepository = new ScopedAppRepository(server.AppRepository, appId, notFoundApp);
                return new AppRouteResult(
                    appId,
                    null,
                    notFoundApp,
                    scopedRepository,
                    chrome,
                    (int)HttpStatusCode.NotFound
                );
            }
        }

        return new AppRouteResult(
            appId,
            null,
            resolvedApp,
            server.AppRepository,
            chrome,
            null
        );
    }

    private static string? GetChromeDefaultAppId(AppDescriptor chromeApp)
    {
        if (chromeApp.CreateApp() is DefaultSidebarChrome chromeView)
        {
            return chromeView.Settings.DefaultAppId;
        }
        return null;
    }
}
