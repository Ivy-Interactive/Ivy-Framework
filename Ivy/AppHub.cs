using System.Text.Json.Nodes;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Chrome;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Exceptions;
using Ivy.Core.HttpTunneling;
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
    private AppArgs GetAppArgs(string connectionId, string appId, string? navigationAppId, HttpContext httpContext, string requestScheme)
    {
        string? appArgs = null;
        if (httpContext.Request.Query.TryGetValue("appArgs", out var appArgsParam))
        {
            appArgs = appArgsParam.ToString().NullIfEmpty();
        }

        return new AppArgs(connectionId, appId, navigationAppId, appArgs ?? server.Args?.Args, requestScheme, httpContext.Request.Host.Value!);
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

            var tunneledHttpHandler = new TunneledHttpMessageHandler(clientProvider, Context.ConnectionId);
            appServices.AddSingleton<HttpMessageHandler>(tunneledHttpHandler);

            var request = httpContext.Request;
            var requestScheme = request.Scheme;
            if (request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto))
            {
                requestScheme = forwardedProto.ToString();
            }

            if (server.AuthProviderType != null)
            {
                var authProvider = server.ServiceProvider!.GetService<IAuthProvider>() ?? throw new Exception("IAuthProvider not found");
#if DEBUG
                authProvider = new CheckedAuthProvider(authProvider);
#endif

                var authSession = AuthHelper.GetAuthSession(httpContext, tunneledHttpHandler);
                var authService = new AuthService(authProvider, authSession, clientProvider, sessionStore);

                var oldSession = authSession.TakeSnapshot();
                await TimeoutHelper.WithTimeoutAsync(
                    ct => authProvider.InitializeAsync(authSession, requestScheme, request.Host.Value!, ct),
                    Context.ConnectionAborted);
                if (authSession.HasChangedSince(oldSession))
                {
                    authService.SetAuthCookies(reloadPage: false);
                }

                appServices.AddSingleton<IAuthService>(s => authService);

                oldSession = authSession.TakeSnapshot();
                try
                {
                    if (!string.IsNullOrEmpty(oldSession.AuthToken?.AccessToken))
                    {
                        var isValid = await TimeoutHelper.WithTimeoutAsync(
                            ct => authProvider.ValidateAccessTokenAsync(authSession, ct),
                            Context.ConnectionAborted);

                        if (!isValid)
                        {
                            await authService.RefreshAccessTokenAsync(Context.ConnectionAborted);
                        }
                    }
                    else
                    {
                        authSession.AuthToken = null;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Auth validation or refresh failed during connection setup.");
                    authSession.AuthToken = null;
                }

                if (authSession.AuthToken == null && parentId != null)
                {
                    await authService.LogoutAsync(Context.ConnectionAborted);
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

            var appArgs = GetAppArgs(Context.ConnectionId, routeResult.AppId, routeResult.NavigationAppId, httpContext, requestScheme);

            logger.LogInformation("Connected: {ConnectionId} [{AppId}]", Context.ConnectionId, routeResult.AppId);

            appServices.AddSingleton(appArgs);
            appServices.AddSingleton(routeResult.AppDescriptor);

            appServices.AddTransient<IWebhookRegistry, WebhookController>();
            appServices.AddTransient(_ => new SignalRouter(sessionStore));

            var serviceProvider = new CompositeServiceProvider(appServices.BuildServiceProvider(), server.ServiceProvider!);

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
                    clientProvider.Sender.Send("Update", changes);
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
                    title = "Internal Server Error",
                    description = ex.Message,
                    stackTrace = ex.StackTrace,
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

            // Cancel all pending HTTP tunnel requests for this connection
            HttpTunnelingController.CancelRequestsForConnection(Context.ConnectionId, "SignalR connection closed");

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

    async Task AbandonConnection(string connectionId, bool resetTokenAndReload)
    {
        var session = sessionStore.Sessions[connectionId];
        await SessionHelpers.AbandonSessionAsync(sessionStore, session, contentBuilder, resetTokenAndReload, triggerMachineReload: true, logger, "AuthRefreshLoop");
    }

    private async Task AuthRefreshLoopAsync(string connectionId, CancellationToken cancellationToken)
    {
        var state = AuthRefreshState.Initial;
        var consecutiveErrors = 0;

        while (true)
        {
            try
            {
                var session = sessionStore.Sessions[connectionId];
                var authService = session.AppServices.GetRequiredService<IAuthService>();
                var authProvider = session.AppServices.GetRequiredService<IAuthProvider>();

                var authSession = authService.GetAuthSession();

                switch (state)
                {
                    case AuthRefreshState.Initial:
                        logger.LogInformation("AuthRefreshLoop: Initialized for {ConnectionId}.", connectionId);
                        state = authSession.AuthToken == null
                            ? AuthRefreshState.HasNoToken
                            : AuthRefreshState.HasToken;
                        break;

                    case AuthRefreshState.HasNoToken:
                        if (authSession.AuthToken != null)
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
                            if (authSession.AuthToken == null)
                            {
                                logger.LogError("AuthRefreshLoop: Token lost for {ConnectionId}.", connectionId);
                                await AbandonConnection(connectionId, resetTokenAndReload: true);
                                return;
                            }

                            var isValid = await TimeoutHelper.WithTimeoutAsync(
                                ct => authProvider.ValidateAccessTokenAsync(authSession, ct),
                                cancellationToken);

                            if (!isValid)
                            {
                                state = AuthRefreshState.TokenInvalid;
                            }
                            else
                            {
                                var lifetime = await TimeoutHelper.WithTimeoutAsync(
                                    ct => authProvider.GetAccessTokenLifetimeAsync(authSession, ct),
                                    cancellationToken);

                                TimeSpan renewalMargin;
                                if (lifetime != null && lifetime.NotBefore != null && lifetime.Expires != null && lifetime.Expires - lifetime.NotBefore is { } duration && duration < TimeSpan.FromMinutes(3))
                                {
                                    renewalMargin = duration / 6;
                                }
                                else
                                {
                                    renewalMargin = TimeSpan.FromMinutes(2);
                                }

                                if (lifetime?.Expires != null && lifetime.Expires - renewalMargin < DateTimeOffset.UtcNow)
                                {
                                    state = AuthRefreshState.TokenExpired;
                                }
                                else
                                {
                                    // Token is valid, wait until close to expiration
                                    var waitUntil = (lifetime?.Expires ?? DateTimeOffset.UtcNow.AddMinutes(15)) - renewalMargin;
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
                            var oldSession = authSession.TakeSnapshot();
                            await authService.RefreshAccessTokenAsync(cancellationToken);
                            if (state == AuthRefreshState.TokenInvalid && authSession.AuthToken == oldSession.AuthToken)
                            {
                                // This case should only ever happen if the auth provider implementation is bad (i.e. it returns the same invalid token on refresh).
                                // It is still good to handle it here to avoid an infinite loop.
                                logger.LogError("AuthRefreshLoop: Invalid token object unchanged after refresh for {ConnectionId}.", connectionId);
                                await authService.LogoutAsync(cancellationToken);
                            }
                            if (authSession.AuthToken == null)
                            {
                                logger.LogError("AuthRefreshLoop: Token refresh failed for {ConnectionId}, aborting connection.", connectionId);
                                // Setting the token and reloading will have already happened above if null.
                                await AbandonConnection(connectionId, resetTokenAndReload: false);
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
                    await AbandonConnection(connectionId, resetTokenAndReload: true);
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