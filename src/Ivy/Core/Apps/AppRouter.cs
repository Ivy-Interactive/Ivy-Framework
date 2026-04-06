using System.Net;
using Microsoft.AspNetCore.Http;

namespace Ivy.Core.Apps;

public record AppRouteResult(
    string AppId,
    string? NavigationAppId,
    AppDescriptor AppDescriptor,
    IAppRepository AppRepository,
    bool ShowAppShell,
    int? HttpStatusCode,
    string? ArgsJson = null
);

public class AppRouter(global::Ivy.Server server)
{
    public AppRouteResult Resolve(HttpContext httpContext)
    {
        var appShell = GetAppShell(httpContext);
        var parentId = GetParentId(httpContext);
        var (appId, navigationAppId) = GetAppId(httpContext, appShell);

        return Resolve(appId, navigationAppId, parentId, appShell);
    }

    private AppRouteResult Resolve(string? appId, string? navigationAppId, string? parentId, bool appShell)
    {
        if (string.IsNullOrEmpty(appId))
        {
            return ResolveDefaultApp(navigationAppId, parentId, appShell);
        }

        return ResolveExplicitApp(appId, appShell);
    }

    internal static bool GetAppShell(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue("shell", out var shellParam))
        {
            return !shellParam.ToString().Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        // Backwards compatibility: accept the old "chrome" parameter name
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

    private static (string? AppId, string? NavigationAppId) GetAppId(HttpContext httpContext, bool appShell)
    {
        string? appId = null;
        string? navigationAppId = null;

        if (httpContext.Request.Query.TryGetValue("appId", out var appIdParam))
        {
            var id = appIdParam.ToString().TrimEnd('/');
            if (string.IsNullOrEmpty(id) || id == AppIds.AppShell || id == AppIds.Auth || id == AppIds.Default)
            {
                id = null;
            }

            if (appShell)
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

    private AppRouteResult ResolveDefaultApp(string? navigationAppId, string? parentId, bool appShell)
    {
        AppDescriptor defaultAppDescriptor;
        string? appId;
        try
        {
            defaultAppDescriptor = server.AppRepository.GetAppOrDefault(null);
            appId = server.DefaultAppId ?? defaultAppDescriptor.Id;
        }
        catch (InvalidOperationException)
        {
            var errorApp = server.AppRepository.GetApp(AppIds.ErrorNotFound);
            if (errorApp == null)
                throw;
            var noAppsArgs = ErrorAppArgs.ToArgsJson(ErrorAppArgs.ForNoApps());
            return new AppRouteResult(
                AppIds.ErrorNotFound,
                null,
                errorApp,
                server.AppRepository,
                appShell,
                (int)HttpStatusCode.NotFound,
                noAppsArgs
            );
        }

        var appShellApp = server.AppRepository.GetAppOrDefault(AppIds.AppShell);

        string? resolvedNavigationAppId = navigationAppId;

        if (appShellApp?.Id == AppIds.AppShell)
        {
            string? appShellDefaultAppId = GetAppShellDefaultAppId(appShellApp);

            if (appId == AppIds.AppShell && (parentId != null || !appShell))
            {
                appId = appShellDefaultAppId ?? GetFirstVisibleAppId();
            }
            else if (appShell && navigationAppId == null)
            {
                resolvedNavigationAppId = appShellDefaultAppId;
            }
        }

        if (!string.IsNullOrEmpty(resolvedNavigationAppId))
        {
            return ResolveNavigationApp(appId, resolvedNavigationAppId, appShellApp, appShell);
        }

        var appDescriptor = server.GetApp(appId ?? AppIds.Default);

        return new AppRouteResult(
            appId ?? AppIds.Default,
            resolvedNavigationAppId,
            appDescriptor,
            server.AppRepository,
            appShell,
            null
        );
    }

    private AppRouteResult ResolveNavigationApp(string? appId, string navigationAppId, AppDescriptor? appShellApp, bool appShell)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(navigationAppId);

        if (resolvedApp.Id != navigationAppId)
        {
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                var scopedRepository = new ScopedAppRepository(server.AppRepository, navigationAppId, notFoundApp);
                var notFoundArgs = ErrorAppArgs.ToArgsJson(ErrorAppArgs.ForNotFound());

                if (appShellApp?.Id != AppIds.AppShell)
                {
                    return new AppRouteResult(
                        appId ?? AppIds.Default,
                        navigationAppId,
                        notFoundApp,
                        scopedRepository,
                        appShell,
                        (int)HttpStatusCode.NotFound,
                        notFoundArgs
                    );
                }

                var appDescriptor = server.GetApp(appId ?? AppIds.Default);
                return new AppRouteResult(
                    appId ?? AppIds.Default,
                    navigationAppId,
                    appDescriptor,
                    scopedRepository,
                    appShell,
                    null
                );
            }
        }

        var descriptor = server.GetApp(appId ?? AppIds.Default);
        return new AppRouteResult(
            appId ?? AppIds.Default,
            navigationAppId,
            descriptor,
            server.AppRepository,
            appShell,
            null
        );
    }

    private AppRouteResult ResolveExplicitApp(string appId, bool appShell)
    {
        var resolvedApp = server.AppRepository.GetAppOrDefault(appId);

        if (resolvedApp.Id != appId)
        {
            var notFoundApp = server.AppRepository.GetAppOrDefault(AppIds.ErrorNotFound);

            if (notFoundApp.Id == AppIds.ErrorNotFound)
            {
                var scopedRepository = new ScopedAppRepository(server.AppRepository, appId, notFoundApp);
                var notFoundArgs = ErrorAppArgs.ToArgsJson(ErrorAppArgs.ForNotFound());
                return new AppRouteResult(
                    appId,
                    null,
                    notFoundApp,
                    scopedRepository,
                    appShell,
                    (int)HttpStatusCode.NotFound,
                    notFoundArgs
                );
            }
        }

        return new AppRouteResult(
            appId,
            null,
            resolvedApp,
            server.AppRepository,
            appShell,
            null
        );
    }

    private static string? GetAppShellDefaultAppId(AppDescriptor appShellApp)
    {
        if (appShellApp.CreateApp() is DefaultSidebarAppShell appShellView)
        {
            return appShellView.Settings.DefaultAppId;
        }
        return null;
    }

    private string? GetFirstVisibleAppId()
    {
        return server.AppRepository.All()
            .Where(app => app.IsVisible && !AppIds.ShouldNotBeAutoDefaultApps.Contains(app.Id))
            .OrderBy(app => app.Order)
            .ThenBy(app => app.Title)
            .Select(app => app.Id)
            .FirstOrDefault();
    }
}
