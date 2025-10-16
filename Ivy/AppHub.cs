using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Exceptions;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy;

public class AppHub(
    Server server,
    IClientNotifier clientNotifier,
    IContentBuilder contentBuilder,
    AppSessionStore sessionStore,
    ILogger<AppHub> logger
    ) : Hub
{
    public static string GetAppId(Server server, HttpContext httpContext)
    {
        string? appId = server.DefaultAppId;

        if (httpContext!.Request.Query.ContainsKey("appId"))
        {
            appId = httpContext!.Request.Query["appId"].ToString();
        }

        if (string.IsNullOrEmpty(appId))
        {
            appId = server.DefaultAppId ?? server.AppRepository.GetAppOrDefault(null).Id;
        }

        return appId;
    }

    public static string GetMachineId(HttpContext httpContext)
    {
        if (httpContext!.Request.Query.ContainsKey("machineId"))
        {
            return httpContext!.Request.Query["machineId"].ToString().NullIfEmpty() ?? throw new Exception("Missing machineId in request.");
        }

        throw new Exception("Missing machineId in request.");
    }

    public static string? GetParentId(HttpContext httpContext)
    {
        if (httpContext!.Request.Query.ContainsKey("parentId"))
        {
            return httpContext!.Request.Query["parentId"].ToString().NullIfEmpty();
        }

        return null;
    }

    public AppArgs GetAppArgs(string connectionId, string appId, HttpContext httpContext)
    {
        string? appArgs = null;
        if (httpContext!.Request.Query.ContainsKey("appArgs"))
        {
            appArgs = httpContext!.Request.Query["appArgs"].ToString().NullIfEmpty();
        }

        HttpRequest request = httpContext.Request;
        return new AppArgs(connectionId, appId, appArgs ?? server.Args?.Args, request.Scheme, request.Host.Value!);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var appServices = new ServiceCollection();

            var httpContext = Context.GetHttpContext()!;
            var appId = GetAppId(server, httpContext);

            var clientProvider = new ClientProvider(new ClientSender(clientNotifier, Context.ConnectionId));

            if (server.Services.All(sd => sd.ServiceType != typeof(IExceptionHandler)))
            {
                appServices.AddSingleton<IExceptionHandler>(_ => new ExceptionHandlerPipeline()
                    .Use(new ConsoleExceptionHandler()).Use(new ClientExceptionHandler(clientProvider))
                    .Build());
            }

            appServices.AddSingleton(typeof(IContentBuilder), contentBuilder);
            appServices.AddSingleton(typeof(IAppRepository), server.AppRepository);
            appServices.AddSingleton(typeof(IDownloadService), new DownloadService(Context.ConnectionId));
            appServices.AddSingleton(typeof(IUploadService), new UploadService(Context.ConnectionId));
            appServices.AddSingleton(typeof(IClientProvider), clientProvider);

            if (server.AuthProviderType != null)
            {
                var authProvider = server.Services.BuildServiceProvider().GetService<IAuthProvider>() ?? throw new Exception("IAuthProvider not found");
                authProvider.SetHttpContext(httpContext);

                var oldAuthToken = GetAuthToken(httpContext);
                var authService = new AuthService(authProvider!, oldAuthToken);
                appServices.AddSingleton<IAuthService>(s => authService);

                AuthToken? authToken = oldAuthToken;
                if (!string.IsNullOrEmpty(oldAuthToken?.AccessToken))
                {
                    if (!await authProvider.ValidateAccessTokenAsync(oldAuthToken.AccessToken))
                    {
                        authToken = await authService.RefreshAccessTokenAsync();
                    }
                }
                else
                {
                    authToken = null;
                }

                if (authToken != oldAuthToken)
                {
                    clientProvider.SetAuthToken(authToken, reloadPage: false);
                }

                if (authToken == null)
                {
                    appId = AppIds.Auth;
                }
            }

            var appArgs = GetAppArgs(Context.ConnectionId, appId, httpContext);
            var appDescriptor = server.GetApp(appId);

            logger.LogInformation($"Connected: {Context.ConnectionId} [{appId}]");

            appServices.AddSingleton(appArgs);
            appServices.AddSingleton(appDescriptor);

            appServices.AddTransient<IWebhookRegistry, WebhookController>();
            appServices.AddTransient<SignalRouter>(_ => new SignalRouter(sessionStore));

            var serviceProvider = new CompositeServiceProvider(appServices, server.Services);

            var app = appDescriptor.CreateApp();

            var widgetTree = new WidgetTree(app, contentBuilder, serviceProvider);

            var appState = new AppSession
            {
                AppId = appId,
                MachineId = GetMachineId(httpContext),
                ParentId = GetParentId(httpContext),
                AppDescriptor = appDescriptor,
                App = app,
                ConnectionId = Context.ConnectionId,
                WidgetTree = widgetTree,
                ContentBuilder = contentBuilder,
                AppServices = serviceProvider,
                LastInteraction = DateTime.UtcNow,
            };

            async void OnWidgetTreeChanged(WidgetTreeChanged[] changes)
            {
                try
                {
                    logger.LogDebug($"> Update");
                    await clientNotifier.NotifyClientAsync(appState.ConnectionId, "Update", changes);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{ConnectionId}", appState.ConnectionId);
                }
            }

            appState.TrackDisposable(widgetTree.Subscribe(OnWidgetTreeChanged));

            sessionStore.Sessions[Context.ConnectionId] = appState;

            if (server.AuthProviderType != null && appId != AppIds.Auth)
            {
                _ = Task.Run(() => AuthRefreshLoopAsync(Context.ConnectionId, Context.ConnectionAborted));
            }
            await base.OnConnectedAsync();

            try
            {
                await widgetTree.BuildAsync();
                logger.LogInformation($"Refresh: {Context.ConnectionId} [{appId}]");
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = widgetTree.GetWidgets().Serialize()
                });
            }
            catch (Exception e)
            {
                var tree = new WidgetTree(new ErrorView(e), contentBuilder, serviceProvider);
                await tree.BuildAsync();
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = tree.GetWidgets().Serialize()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect client {ConnectionId}", Context.ConnectionId);

            try
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    title = "Internal Server Error",
                    description = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            catch
            {
                logger.LogError("Could not send error message to client {ConnectionId}", Context.ConnectionId);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (exception != null)
            {
                logger.LogWarning(exception, "Client {ConnectionId} disconnected with error", Context.ConnectionId);
            }
            else
            {
                logger.LogInformation("Client {ConnectionId} disconnected normally", Context.ConnectionId);
            }

            if (sessionStore.Sessions.TryRemove(Context.ConnectionId, out var appState))
            {
                try
                {
                    appState.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error disposing app state for {ConnectionId}", Context.ConnectionId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during disconnection for {ConnectionId}", Context.ConnectionId);
        }
    }

    private async Task AuthRefreshLoopAsync(string connectionId, CancellationToken abort)
    {
        try
        {
            while (true)
            {
                var session = sessionStore.Sessions[connectionId];
                var authService = session.AppServices.GetRequiredService<IAuthService>();
                var authProvider = session.AppServices.GetRequiredService<IAuthProvider>();
                var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();

                var oldToken = authService.GetCurrentToken();
                if (oldToken == null)
                {
                    Console.WriteLine("AuthRefreshLoop: No token, waiting 5 minutes.");
                    await Task.Delay(TimeSpan.FromMinutes(5), abort);
                    continue;
                }

                DateTimeOffset? expiresAt;
                var refreshNeeded = false;
                if (await authProvider.ValidateAccessTokenAsync(oldToken.AccessToken))
                {
                    expiresAt = await authProvider.GetTokenExpiration(oldToken);
                    if (expiresAt == null || expiresAt < DateTimeOffset.UtcNow.AddMinutes(2))
                    {
                        refreshNeeded = true;
                    }
                }
                else
                {
                    refreshNeeded = true;
                }

                AuthToken? newToken = oldToken;
                if (refreshNeeded)
                {
                    newToken = await authService.RefreshAccessTokenAsync();
                }
                expiresAt = newToken != null
                    ? await authProvider.GetTokenExpiration(newToken)
                    : null;

                var earliestWake = DateTimeOffset.UtcNow.AddMinutes(5);
                var latestWake = DateTimeOffset.UtcNow.AddMinutes(30);
                var nextUpdate = expiresAt?.AddMinutes(-2) ?? DateTimeOffset.UtcNow.AddMinutes(15);
                if (nextUpdate < earliestWake)
                {
                    nextUpdate = earliestWake;
                }
                if (nextUpdate > latestWake)
                {
                    nextUpdate = latestWake;
                }

                var reloadPage = string.IsNullOrEmpty(newToken?.AccessToken);

                if (oldToken != newToken)
                {
                    Console.WriteLine("AuthRefreshLoop: Token changed, updating client. Reloading: {0}", reloadPage);
                    clientProvider.SetAuthToken(newToken, reloadPage);
                }

                if (reloadPage)
                {
                    try
                    {
                        Console.WriteLine("AuthRefreshLoop: closing connection");
                        // Close the connection to be extra safe.
                        Context.Abort();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignore
                    }
                    return;
                }

                Console.WriteLine("AuthRefreshLoop: next update at {0}...", nextUpdate);
                await Task.Delay(nextUpdate - DateTimeOffset.UtcNow, abort);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during auth refresh loop for {ConnectionId}", connectionId);
            try
            {
                // Close the connection to be extra safe.
                Context.Abort();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }
    }

    public void HotReload()
    {
        if (sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            appSession.LastInteraction = DateTime.UtcNow;
            logger.LogInformation($"HotReload: {Context.ConnectionId} [{appSession.AppId}]");
            try
            {
                appSession.WidgetTree.HotReload();
            }
            catch (Exception e)
            {
                logger.LogError(e, "HotReload failed.");
            }
        }
        else
        {
            logger.LogWarning($"HotReload: {Context.ConnectionId} [Not Found]");
        }
    }

    public void Event(string eventName, string widgetId, JsonArray? args)
    {
        logger.LogInformation($"Event: {eventName} {widgetId} {args}");
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogWarning($"Event: {eventName} {widgetId} [AppSession Not Found]");
            return;
        }

        try
        {
            appSession.LastInteraction = DateTime.UtcNow;
            if (!appSession.WidgetTree.TriggerEvent(widgetId, eventName, args ?? new JsonArray()))
            {
                logger.LogWarning($"Event '{eventName}' for Widget '{widgetId}' not found.");
            }
        }
        catch (Exception e)
        {
            var exceptionHandler = appSession.AppServices.GetService<IExceptionHandler>()!;
            exceptionHandler.HandleException(e);
        }
    }

    private AuthToken? GetAuthToken(HttpContext httpContext)
    {
        var cookies = httpContext.Request.Cookies;
        var authToken = cookies["auth_token"].NullIfEmpty();
        if (authToken == null)
        {
            return null;
        }

        try
        {
            var token = JsonSerializer.Deserialize<AuthToken>(authToken);
            if (token == null)
            {
                return null;
            }

            if (token.RefreshToken == null)
            {
                var refreshToken = cookies["auth_ext_refresh_token"].NullIfEmpty();
                return token with { RefreshToken = refreshToken };
            }

            return token;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to deserialize AuthToken from cookies.");
            return null;
        }
    }
}

public class ClientSender(IClientNotifier clientNotifier, string connectionId) : IClientSender
{
    public void Send(string method, object? data)
    {
        // Fire and forget, but handle exceptions to prevent crashes
        _ = Task.Run(async () =>
        {
            try
            {
                await clientNotifier.NotifyClientAsync(connectionId, method, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send {method} to client {connectionId}: {ex.Message}");
            }
        });
    }
}

public class ClientProvider(IClientSender sender) : IClientProvider
{
    public IClientSender Sender { get; set; } = sender;
}
