using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Chrome;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Exceptions;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Views;
using Ivy.Views.DataTables;
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
    ILogger<AppHub> logger,
    IQueryableRegistry queryableRegistry
    ) : Hub
{
    private AppArgs GetAppArgs(string connectionId, string appId, string? navigationAppId, HttpContext httpContext)
    {
        string? appArgs = null;
        if (httpContext.Request.Query.TryGetValue("appArgs", out var appArgsParam))
        {
            appArgs = appArgsParam.ToString().NullIfEmpty();
        }

        var request = httpContext.Request;
        return new AppArgs(connectionId, appId, navigationAppId, appArgs ?? server.Args?.Args, request.Scheme, request.Host.Value!);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var appServices = new ServiceCollection();

            var httpContext = Context.GetHttpContext()!;
            var parentId = AppRouter.GetParentId(httpContext);

            var clientProvider = new ClientProvider(new ClientSender(clientNotifier, Context.ConnectionId));

            if (server.Services.All(sd => sd.ServiceType != typeof(IExceptionHandler)))
            {
                appServices.AddSingleton(_ => new ExceptionHandlerPipeline()
                    .Use(new ConsoleExceptionHandler()).Use(new ClientExceptionHandler(clientProvider))
                    .Build());
            }

            appServices.AddSingleton(contentBuilder);
            appServices.AddSingleton<IAppRepository>(server.AppRepository);
            appServices.AddSingleton<IDownloadService>(new DownloadService(Context.ConnectionId));
            appServices.AddSingleton<IDataTableService>(new DataTableConnectionService(
                queryableRegistry,
                server.Args,
                Context.ConnectionId));
            appServices.AddSingleton<IClientProvider>(clientProvider);
            appServices.AddSingleton<IUploadService>(new UploadService(Context.ConnectionId, clientProvider));

            if (server.AuthProviderType != null)
            {
                var authProvider = server.Services.BuildServiceProvider().GetService<IAuthProvider>() ?? throw new Exception("IAuthProvider not found");
                authProvider.SetHttpContext(httpContext);

                var oldAuthToken = AuthHelper.GetAuthToken(httpContext);
                var authService = new AuthService(authProvider!, oldAuthToken);
                appServices.AddSingleton<IAuthService>(s => authService);

                AuthToken? authToken = oldAuthToken;
                try
                {
                    if (!string.IsNullOrEmpty(oldAuthToken?.AccessToken))
                    {
                        var isValid = await TimeoutHelper.WithTimeoutAsync(
                            ct => authProvider.ValidateAccessTokenAsync(oldAuthToken.AccessToken, ct),
                            Context.ConnectionAborted);

                        if (!isValid)
                        {
                            authToken = await TimeoutHelper.WithTimeoutAsync(
                                authService.RefreshAccessTokenAsync,
                                Context.ConnectionAborted);
                        }
                    }
                    else
                    {
                        authToken = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Auth validation or refresh failed during connection setup.");
                    authToken = null;
                }

                if (authToken != oldAuthToken || parentId != null)
                {
                    clientProvider.SetAuthToken(authToken, reloadPage: parentId != null && authToken == null);
                }
            }

            var appRouter = new AppRouter(server);
            var routeResult = appRouter.Resolve(httpContext);

            // Override to Auth app if authentication failed
            if (server.AuthProviderType != null)
            {
                var authService = appServices.BuildServiceProvider().GetService<IAuthService>();
                if (authService?.GetCurrentToken() == null)
                {
                    var authApp = server.AppRepository.GetAppOrDefault(AppIds.Auth);
                    routeResult = routeResult with
                    {
                        AppId = AppIds.Auth,
                        AppDescriptor = authApp
                    };
                }
            }

            appServices.AddSingleton(routeResult.AppRepository);

            var appArgs = GetAppArgs(Context.ConnectionId, routeResult.AppId, routeResult.NavigationAppId, httpContext);

            logger.LogInformation("Connected: {ConnectionId} [{AppId}]", Context.ConnectionId, routeResult.AppId);

            appServices.AddSingleton(appArgs);
            appServices.AddSingleton(routeResult.AppDescriptor);

            appServices.AddTransient<IWebhookRegistry, WebhookController>();
            appServices.AddTransient(_ => new SignalRouter(sessionStore));

            var serviceProvider = new CompositeServiceProvider(appServices, server.Services);

            var app = routeResult.AppDescriptor.CreateApp();

            var widgetTree = new WidgetTree(app, contentBuilder, serviceProvider);

            var appState = new AppSession
            {
                AppId = routeResult.AppId,
                MachineId = AppRouter.GetMachineId(httpContext),
                ParentId = parentId,
                AppDescriptor = routeResult.AppDescriptor,
                App = app,
                ConnectionId = Context.ConnectionId,
                WidgetTree = widgetTree,
                ContentBuilder = contentBuilder,
                AppServices = serviceProvider,
                LastInteraction = DateTime.UtcNow,
            };

            var connectionAborted = Context.ConnectionAborted;
            appState.EventQueue = new EventDispatchQueue(connectionAborted);

            if (parentId == null)
            {
                clientProvider.SetRootAppId(routeResult.AppId);
                bool isNotFoundPage = routeResult.AppDescriptor.Id == AppIds.ErrorNotFound;

                if (routeResult.AppId != AppIds.Chrome && !isNotFoundPage)
                {
                    var navigateArgs = new NavigateArgs(routeResult.AppId, Chrome: routeResult.ShowChrome);
                    clientProvider.Redirect(navigateArgs.GetUrl(), replaceHistory: true);
                }
            }

            void OnWidgetTreeChanged(WidgetTreeChanged[] changes)
            {
                try
                {
                    logger.LogDebug("> Update");
                    appState.PendingUpdate = changes;
                    if (appState.UpdateScheduled) return;
                    appState.UpdateScheduled = true;

                    // Use Task.Run instead of EventQueue to prevent UI updates from being blocked by long-running event handlers
                    _ = Task.Run(async () =>
                    {
                        try { await Task.Delay(16, connectionAborted); }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            var payload = appState.PendingUpdate;
                            if (payload != null)
                            {
                                clientProvider.Sender.Send("Update", payload);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "{ConnectionId}", appState.ConnectionId);
                        }
                        finally
                        {
                            appState.UpdateScheduled = false;
                            appState.PendingUpdate = null;
                        }
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{ConnectionId}", appState.ConnectionId);
                }
            }

            appState.TrackDisposable(widgetTree.Subscribe(OnWidgetTreeChanged));

            sessionStore.Sessions[Context.ConnectionId] = appState;

            var connectionId = Context.ConnectionId;

            await base.OnConnectedAsync();

            try
            {
                await widgetTree.BuildAsync();
                logger.LogInformation("Refresh: {ConnectionId} [{AppId}]", Context.ConnectionId, routeResult.AppId);
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = widgetTree.GetWidgets().Serialize()
                }, cancellationToken: connectionAborted);
            }
            catch (Exception e)
            {
                var tree = new WidgetTree(new ErrorView(e), contentBuilder, serviceProvider);
                await tree.BuildAsync();
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = tree.GetWidgets().Serialize()
                }, cancellationToken: connectionAborted);
            }

            if (server.AuthProviderType != null && routeResult.AppId != AppIds.Auth)
            {
                _ = Task.Run(() => AuthRefreshLoopAsync(connectionId, connectionAborted), connectionAborted);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect client {ConnectionId}", Context.ConnectionId);

            try
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    viewOverride = new NotFoundApp()
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
                    try
                    {
                        var cp = appState.AppServices.GetService<IClientProvider>();
                        if (cp?.Sender is ClientSender cs)
                        {
                            cs.Dispose();
                        }
                    }
                    catch
                    {
                        // ignored
                    }

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

    enum AuthRefreshState
    {
        Initial,
        HasToken,
        HasNoToken,
        TokenExpired,
        TokenInvalid,
    }

    private async Task AuthRefreshLoopAsync(string connectionId, CancellationToken cancellationToken)
    {
        var state = AuthRefreshState.Initial;
        var consecutiveErrors = 0;

        // Replace connection's widget tree with an error view, so an unauthenticated user cannot interact with the real app.
        // This is intended mainly as a safeguard against malicious clients (e.g., those which ignore messages that should trigger a page reload and/or cookie updates).
        // The error page this provides is not very user-friendly, but in practice it should very rarely appear for a legitimate user.
        async Task AbandonConnection(bool resetTokenAndReload)
        {
            try
            {
                var displayException = new Exception("Your session is no longer valid. Please log in again.");
                var session = sessionStore.Sessions[connectionId];
                var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();
                if (resetTokenAndReload)
                {
                    clientProvider.SetAuthToken(null, reloadPage: true);
                }
                session.WidgetTree = new WidgetTree(new ErrorView(displayException), contentBuilder, session.AppServices);
                await session.WidgetTree.BuildAsync();
                clientProvider.Sender.Send("Refresh", new
                {
                    Widgets = session.WidgetTree.GetWidgets().Serialize()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuthRefreshLoop: Error sending session expired message to {ConnectionId}", connectionId);
            }
        }

        while (true)
        {
            try
            {
                var session = sessionStore.Sessions[connectionId];
                var authService = session.AppServices.GetRequiredService<IAuthService>();
                var authProvider = session.AppServices.GetRequiredService<IAuthProvider>();
                var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();

                var token = authService.GetCurrentToken();

                switch (state)
                {
                    case AuthRefreshState.Initial:
                        logger.LogInformation("AuthRefreshLoop: Initialized for {ConnectionId}.", connectionId);
                        state = token == null
                            ? AuthRefreshState.HasNoToken
                            : AuthRefreshState.HasToken;
                        break;

                    case AuthRefreshState.HasNoToken:
                        if (token != null)
                        {
                            state = AuthRefreshState.HasToken;
                        }
                        else
                        {
                            logger.LogInformation("AuthRefreshLoop: No token for {ConnectionId}, waiting 5 minutes.", connectionId);
                            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                        }
                        break;

                    case AuthRefreshState.HasToken:
                        {
                            if (token == null)
                            {
                                logger.LogError("AuthRefreshLoop: Token lost for {ConnectionId}.", connectionId);
                                await AbandonConnection(resetTokenAndReload: true);
                                return;
                            }

                            var isValid = await TimeoutHelper.WithTimeoutAsync(
                                ct => authProvider.ValidateAccessTokenAsync(token.AccessToken, ct),
                                cancellationToken);

                            if (!isValid)
                            {
                                state = AuthRefreshState.TokenInvalid;
                            }
                            else
                            {
                                var expiresAt = await TimeoutHelper.WithTimeoutAsync(
                                    ct => authProvider.GetTokenExpiration(token, ct),
                                    cancellationToken);

                                if (expiresAt != null && expiresAt < DateTimeOffset.UtcNow.AddMinutes(2))
                                {
                                    state = AuthRefreshState.TokenExpired;
                                }
                                else
                                {
                                    // Token is valid, wait until close to expiration
                                    var waitUntil = (expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(15)).AddMinutes(-2);
                                    var delay = waitUntil - DateTimeOffset.UtcNow;

                                    // Don't wait more than `maxDelay`
                                    var maxDelay = TimeSpan.FromHours(2);
                                    if (delay > maxDelay)
                                    {
                                        delay = maxDelay;
                                    }
                                    logger.LogInformation("AuthRefreshLoop: Token valid for {ConnectionId}, next check at {NextCheck}.", connectionId, DateTimeOffset.UtcNow + delay);
                                    await Task.Delay(delay, cancellationToken);
                                }
                            }
                        }
                        break;

                    case AuthRefreshState.TokenExpired:
                    case AuthRefreshState.TokenInvalid:
                        {
                            var newToken = await TimeoutHelper.WithTimeoutAsync(
                                authService.RefreshAccessTokenAsync,
                                cancellationToken);
                            if (state == AuthRefreshState.TokenInvalid && token == newToken)
                            {
                                // This case should only ever happen if the auth provider implementation is bad (i.e. it returns the same invalid token on refresh).
                                // It is still good to handle it here to avoid an infinite loop.
                                logger.LogError("AuthRefreshLoop: Invalid token object unchanged after refresh for {ConnectionId}.", connectionId);
                                newToken = null;
                            }
                            if (token != newToken)
                            {
                                clientProvider.SetAuthToken(newToken, reloadPage: string.IsNullOrEmpty(newToken?.AccessToken));
                            }
                            if (newToken == null)
                            {
                                logger.LogError("AuthRefreshLoop: Token refresh failed for {ConnectionId}, aborting connection.", connectionId);
                                // Setting the token and reloading will have already happened above if null.
                                await AbandonConnection(resetTokenAndReload: false);
                                return;
                            }
                            else
                            {
                                state = AuthRefreshState.HasToken;
                            }
                        }
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("AuthRefreshLoop: cancelled for {ConnectionId}", connectionId);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AuthRefreshLoop: Error during auth refresh loop for {ConnectionId}", connectionId);
                consecutiveErrors++;
                if (consecutiveErrors >= 5)
                {
                    logger.LogError("AuthRefreshLoop: Too many consecutive errors, abandoning connection {ConnectionId}", connectionId);
                    await AbandonConnection(resetTokenAndReload: true);
                    return;
                }
                logger.LogInformation("AuthRefreshLoop: waiting 30 seconds before retrying for {ConnectionId}", connectionId);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                continue;
            }

            consecutiveErrors = 0;
        }
    }

    public void HotReload()
    {
        if (sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            appSession.LastInteraction = DateTime.UtcNow;
            logger.LogInformation("HotReload: {ConnectionId} [{AppId}]", Context.ConnectionId, appSession.AppId);
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
            logger.LogWarning("HotReload: {ConnectionId} [Not Found]", Context.ConnectionId);
        }
    }

    public Task Event(string eventName, string widgetId, JsonArray? args)
    {
        logger.LogDebug("Event: {EventName} {WidgetId} {Args}", eventName, widgetId, args);
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogWarning("Event: {EventName} {WidgetId} [AppSession Not Found]", eventName, widgetId);
            return Task.CompletedTask;
        }

        // Enqueue async event handling to avoid tying up ThreadPool workers
        appSession.EventQueue?.Enqueue(async () =>
        {
            try
            {
                appSession.LastInteraction = DateTime.UtcNow;
                if (!await appSession.WidgetTree.TriggerEventAsync(widgetId, eventName, args ?? new JsonArray()))
                {
                    logger.LogWarning("Event '{EventName}' for Widget '{WidgetId}' not found.", eventName, widgetId);
                }
            }
            catch (Exception e)
            {
                var exceptionHandler = appSession.AppServices.GetService<IExceptionHandler>()!;
                exceptionHandler.HandleException(e);
            }
        });

        return Task.CompletedTask;
    }

    public async Task Navigate(string? appId, HistoryState? state)
    {
        logger.LogInformation("Navigate: {ConnectionId} to [{AppId}] with tab ID {TabId}", Context.ConnectionId, appId, state?.TabId);

        // Find the Chrome session for this connection
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogWarning("Navigate: {ConnectionId} [{AppId}] [AppSession not found]", Context.ConnectionId, appId);
            return;
        }

        var chromeSession = sessionStore.FindChrome(appSession);
        if (chromeSession == null)
        {
            logger.LogWarning("Navigate: {ConnectionId} [{AppId}] [Chrome session not found]", Context.ConnectionId, appId);
            return;
        }

        try
        {
            var navigateSignal = (NavigateSignal)chromeSession.Signals.GetOrAdd(
                typeof(NavigateSignal),
                _ => new NavigateSignal()
            );

            await navigateSignal.Send(new NavigateArgs(appId, TabId: state?.TabId, Purpose: NavigationPurpose.HistoryTraversal));

            logger.LogInformation("Navigate signal sent: {ConnectionId} to [{AppId}]", Context.ConnectionId, appId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send navigate signal: {ConnectionId} to [{AppId}]", Context.ConnectionId, appId);
        }
    }

}

public class ClientSender : IClientSender, IDisposable
{
    private readonly System.Threading.Channels.Channel<(string method, object? data)> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;

    public ClientSender(IClientNotifier clientNotifier, string connectionId)
    {
        var options = new System.Threading.Channels.BoundedChannelOptions(2048)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = System.Threading.Channels.BoundedChannelFullMode.DropOldest
        };
        _channel = System.Threading.Channels.Channel.CreateBounded<(string, object?)>(options);

        _worker = Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _channel.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (_channel.Reader.TryRead(out var msg))
                    {
                        try
                        {
                            await clientNotifier.NotifyClientAsync(connectionId, msg.method, msg.data).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to send {msg.method} to client {connectionId}: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    public void Send(string method, object? data)
    {
        if (!_channel.Writer.TryWrite((method, data)))
        {
            _ = _channel.Writer.WriteAsync((method, data), _cts.Token);
        }
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _channel.Writer.TryComplete();
        }
        catch
        {
            // ignored
        }

        try
        {
            _worker.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // ignored
        }

        _cts.Dispose();
    }
}

public class ClientProvider(IClientSender sender) : IClientProvider
{
    public IClientSender Sender { get; set; } = sender;
}