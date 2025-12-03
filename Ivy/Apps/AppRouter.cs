using Ivy.Chrome;

namespace Ivy.Apps;

public record AppRouteResult(
    string AppId,
    string? NavigationAppId,
    AppDescriptor AppDescriptor,
    IAppRepository AppRepository
);

public class AppRouter(Server server)
{
    public AppRouteResult Resolve(string? appId, string? navigationAppId, string? parentId, bool chrome)
    {
        if (string.IsNullOrEmpty(appId))
        {
            return ResolveDefaultApp(navigationAppId, parentId, chrome);
        }
        return ResolveExplicitApp(appId);
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

        // Check if the requested navigation app exists
        if (!string.IsNullOrEmpty(resolvedNavigationAppId))
        {
            return ResolveNavigationApp(appId, resolvedNavigationAppId, chromeApp);
        }

        // No navigation app requested, use default repository
        var appDescriptor = server.GetApp(appId ?? AppIds.Default);
        return new AppRouteResult(
            appId ?? AppIds.Default,
            resolvedNavigationAppId,
            appDescriptor,
            server.AppRepository
        );
    }

    private AppRouteResult ResolveNavigationApp(string? appId, string navigationAppId, AppDescriptor? chromeApp)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(navigationAppId);

        if (resolvedApp.Id != navigationAppId)
        {
            // App not found, try to find registered 404 app
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                var scopedRepository = new ScopedAppRepository(server.AppRepository, navigationAppId, notFoundApp);

                // If there's no Chrome shell, show the 404 app directly
                if (chromeApp?.Id != AppIds.Chrome)
                {
                    return new AppRouteResult(
                        appId ?? AppIds.Default,
                        navigationAppId,
                        notFoundApp,
                        scopedRepository
                    );
                }

                // With Chrome shell, let Chrome handle displaying the 404
                var appDescriptor = server.GetApp(appId ?? AppIds.Default);
                return new AppRouteResult(
                    appId ?? AppIds.Default,
                    navigationAppId,
                    appDescriptor,
                    scopedRepository
                );
            }
        }

        // App found or no 404 app registered, use default repository
        var descriptor = server.GetApp(appId ?? AppIds.Default);
        return new AppRouteResult(
            appId ?? AppIds.Default,
            navigationAppId,
            descriptor,
            server.AppRepository
        );
    }

    private AppRouteResult ResolveExplicitApp(string appId)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(appId);

        if (resolvedApp.Id != appId)
        {
            // App not found
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                // Scope the repository so that if the view asks for "current app", it gets the 404 app
                var scopedRepository = new ScopedAppRepository(server.AppRepository, appId, notFoundApp);
                return new AppRouteResult(
                    appId,
                    null,
                    notFoundApp,
                    scopedRepository
                );
            }

            // Fallback to default behavior if no 404 app registered
            return new AppRouteResult(
                appId,
                null,
                resolvedApp,
                server.AppRepository
            );
        }

        // App found
        return new AppRouteResult(
            appId,
            null,
            resolvedApp,
            server.AppRepository
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
