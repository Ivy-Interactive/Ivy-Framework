using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Ivy.Core;
using Ivy.Core.Apps;
using Ivy.Core.Auth;
using Ivy.Core.ExternalWidgets;
using Ivy.Core.Helpers;
using Ivy.Core.Exceptions;
using Ivy.Core.HttpTunneling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AppContext = Ivy.AppContext;

namespace Ivy.Core.Server;

public class AppHub(
    global::Ivy.Server server,
    IClientNotifier clientNotifier,
    IContentBuilder contentBuilder,
    AppSessionStore sessionStore,
    ILogger<AppHub> logger,
    IQueryableRegistry queryableRegistry
    ) : Hub
{
    private readonly ConcurrentDictionary<string, Action<OAuthProvider>> _oauthTokenAddedHandlers = new();
    private readonly ConcurrentDictionary<string, Action<OAuthProvider>> _oauthTokenRemovedHandlers = new();
    private readonly ConcurrentDictionary<string, HashSet<OAuthProvider>> _activeOAuthRefreshLoops = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<OAuthProvider, CancellationTokenSource>> _oauthRefreshCancellations = new();

    private AppContext GetAppArgs(string connectionId, string machineId, string appId, string? navigationAppId, HttpContext httpContext, string requestScheme)
    {
        string? appArgs = null;
        if (httpContext.Request.Query.TryGetValue("appArgs", out var appArgsParam))
        {
            appArgs = appArgsParam.ToString().NullIfEmpty();
        }

        return new AppContext(connectionId, machineId, appId, navigationAppId, appArgs ?? server.Args?.Args, requestScheme, httpContext.Request.Host.Value!);
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
                var oauthRegistry = server.ServiceProvider!.GetService<IOAuthTokenHandlerRegistry>();
                var authService = new AuthProviderService(authProvider, authSession, clientProvider, sessionStore, oauthRegistry);

                var oldSession = authSession.TakeSnapshot();
                await TimeoutHelper.WithTimeoutAsync(
                    ct => authProvider.InitializeAsync(authSession, requestScheme, request.Host.Value!, ct),
                    Context.ConnectionAborted);
                if (authSession.HasChangedSince(oldSession))
                {
                    authService.SetAuthCookies(reloadPage: false);
                }

                appServices.AddSingleton<IAuthProviderService>(s => authService);

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
                var authService = appServices.BuildServiceProvider().GetService<IAuthProviderService>();
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

            if (routeResult.AppDescriptor.Title is { } title && routeResult.AppId != AppIds.Chrome && parentId == null)
            {
                clientProvider.SetTitle(title, server.Args.MetaTitle);
            }

            appServices.AddSingleton(routeResult.AppRepository);

            var machineId = AppRouter.GetMachineId(httpContext);

            var appArgs = GetAppArgs(Context.ConnectionId, machineId, routeResult.AppId, routeResult.NavigationAppId, httpContext, requestScheme);

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
                MachineId = machineId,
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

                // Include external widget registry only on initial connection (not for child connections)
                var externalWidgets = parentId == null
                    ? ExternalWidgetRegistry.Instance.GetRegistryForFrontend()
                    : null;

                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = widgetTree.GetWidgets().Serialize(),
                    ExternalWidgets = externalWidgets
                }, cancellationToken: connectionAborted);
            }
            catch (Exception e)
            {
                var tree = new WidgetTree(new ErrorView(e), contentBuilder, serviceProvider);
                await tree.BuildAsync();
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = tree.GetWidgets().Serialize(),
                    ExternalWidgets = (object?)null
                }, cancellationToken: connectionAborted);
            }

            if (server.AuthProviderType != null && routeResult.AppId != AppIds.Auth)
            {
                _ = Task.Run(() => AuthRefreshLoopAsync(connectionId, connectionAborted), connectionAborted);

                // Start a refresh loop for each OAuth provider session
                var authService = appState.AppServices.GetService<IAuthProviderService>();
                if (authService != null)
                {
                    var authSession = authService.GetAuthProviderSession();
                    var oauthProviders = authSession.OAuthProviderSessions.Keys.ToList();

                    // Track active OAuth refresh loops and their cancellation tokens for this connection
                    var activeProviders = _activeOAuthRefreshLoops.GetOrAdd(connectionId, _ => new HashSet<OAuthProvider>());
                    var cancellations = _oauthRefreshCancellations.GetOrAdd(connectionId, _ => new ConcurrentDictionary<OAuthProvider, CancellationTokenSource>());

                    void AddProvider(OAuthProvider provider)
                    {
                        lock (activeProviders)
                        {
                            if (activeProviders.Add(provider))
                            {
                                var cts = CancellationTokenSource.CreateLinkedTokenSource(connectionAborted);
                                cancellations[provider] = cts;
                                _ = Task.Run(() => OAuthTokenRefreshLoopAsync(connectionId, provider, cts.Token), connectionAborted);
                            }
                        }
                    }

                    foreach (var provider in oauthProviders)
                    {
                        AddProvider(provider);
                    }

                    // Subscribe to new OAuth provider sessions being added
                    Action<OAuthProvider> addedHandler = provider =>
                    {
                        // Check if connection is still active
                        if (!sessionStore.Sessions.ContainsKey(connectionId))
                        {
                            return;
                        }

                        AddProvider(provider);
                    };

                    // Subscribe to OAuth provider sessions being removed
                    Action<OAuthProvider> removedHandler = provider =>
                    {
                        logger.LogInformation("Cancelling OAuth token refresh loop for removed provider {Provider} on connection {ConnectionId}", provider, connectionId);

                        if (cancellations.TryRemove(provider, out var cts))
                        {
                            cts.Cancel();
                            cts.Dispose();
                        }

                        lock (activeProviders)
                        {
                            activeProviders.Remove(provider);
                        }
                    };

                    _oauthTokenAddedHandlers[connectionId] = addedHandler;
                    _oauthTokenRemovedHandlers[connectionId] = removedHandler;
                    authSession.OAuthProviderSessionAdded += addedHandler;
                    authSession.OAuthProviderSessionRemoved += removedHandler;
                }
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

            // Clean up OAuth session event subscriptions
            if (_oauthTokenAddedHandlers.TryRemove(Context.ConnectionId, out var addedHandler))
            {
                // Get the auth session and unsubscribe
                if (sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var tempAppState))
                {
                    var authService = tempAppState.AppServices.GetService<IAuthProviderService>();
                    if (authService != null)
                    {
                        var authSession = authService.GetAuthProviderSession();
                        authSession.OAuthProviderSessionAdded -= addedHandler;
                    }
                }
            }

            if (_oauthTokenRemovedHandlers.TryRemove(Context.ConnectionId, out var removedHandler))
            {
                // Get the auth session and unsubscribe
                if (sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var tempAppState))
                {
                    var authService = tempAppState.AppServices.GetService<IAuthProviderService>();
                    if (authService != null)
                    {
                        var authSession = authService.GetAuthProviderSession();
                        authSession.OAuthProviderSessionRemoved -= removedHandler;
                    }
                }
            }

            // Cancel and dispose all OAuth refresh loop cancellation tokens
            if (_oauthRefreshCancellations.TryRemove(Context.ConnectionId, out var cancellations))
            {
                foreach (var kvp in cancellations)
                {
                    try
                    {
                        kvp.Value.Cancel();
                        kvp.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Error cancelling OAuth refresh loop for provider {Provider} on connection {ConnectionId}", kvp.Key, Context.ConnectionId);
                    }
                }
            }

            // Clean up active OAuth refresh loop tracking
            _activeOAuthRefreshLoops.TryRemove(Context.ConnectionId, out _);

            if (sessionStore.Sessions.TryRemove(Context.ConnectionId, out var appState))
            {
                try
                {
                    // Dispose app state first (stops EventDispatchQueue, cleans up widget tree)
                    // so in-flight event handlers finish before the sender is torn down.
                    await appState.DisposeAsync();

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

    enum TokenRefreshState
    {
        Initial,
        HasToken,
        HasNoToken,
        TokenExpired,
        TokenInvalid,
    }

    private async Task TokenRefreshLoopAsync(
        ITokenRefreshStrategy strategy,
        string connectionId,
        CancellationToken cancellationToken)
    {
        var state = TokenRefreshState.Initial;
        var consecutiveErrors = 0;

        while (true)
        {
            try
            {
                switch (state)
                {
                    case TokenRefreshState.Initial:
                        logger.LogInformation("{StrategyName}RefreshLoop: Initialized for {ConnectionId}.", strategy.LoggingName, connectionId);
                        state = strategy.HasToken()
                            ? TokenRefreshState.HasToken
                            : TokenRefreshState.HasNoToken;
                        break;

                    case TokenRefreshState.HasNoToken:
                        if (strategy.HasToken())
                        {
                            state = TokenRefreshState.HasToken;
                        }
                        else
                        {
                            logger.LogInformation("{StrategyName}RefreshLoop: No token for {ConnectionId}, waiting 5 minutes.", strategy.LoggingName, connectionId);
                            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                        }
                        break;

                    case TokenRefreshState.HasToken:
                        {
                            if (!strategy.HasToken())
                            {
                                logger.LogError("{StrategyName}RefreshLoop: Token lost for {ConnectionId}.", strategy.LoggingName, connectionId);
                                var shouldContinue = await strategy.OnTokenLostAsync();
                                if (!shouldContinue)
                                {
                                    return;
                                }
                            }

                            var isValid = await strategy.ValidateTokenAsync(cancellationToken);

                            if (!isValid)
                            {
                                state = TokenRefreshState.TokenInvalid;
                            }
                            else
                            {
                                var lifetime = await strategy.GetTokenLifetimeAsync(cancellationToken);

                                TimeSpan renewalMargin;
                                if (lifetime != null && lifetime.NotBefore != null && lifetime.Expires != null &&
                                    lifetime.Expires - lifetime.NotBefore is { } duration &&
                                    duration < TimeSpan.FromMinutes(3))
                                {
                                    renewalMargin = duration / 6;
                                }
                                else
                                {
                                    renewalMargin = TimeSpan.FromMinutes(2);
                                }

                                if (lifetime?.Expires != null && lifetime.Expires - renewalMargin < DateTimeOffset.UtcNow)
                                {
                                    state = TokenRefreshState.TokenExpired;
                                }
                                else
                                {
                                    // Token is valid, wait until close to expiration
                                    var waitUntil = (lifetime?.Expires ?? DateTimeOffset.UtcNow.AddMinutes(15)) - renewalMargin;
                                    var delay = waitUntil - DateTimeOffset.UtcNow;

                                    // Don't wait more than maxDelay
                                    var maxDelay = TimeSpan.FromHours(2);
                                    if (delay > maxDelay)
                                    {
                                        delay = maxDelay;
                                    }

                                    logger.LogInformation("{StrategyName}RefreshLoop: Token valid for {ConnectionId}, next check at {NextCheck}.", strategy.LoggingName, connectionId, DateTimeOffset.UtcNow + delay);
                                    await Task.Delay(delay, cancellationToken);
                                }
                            }
                        }
                        break;

                    case TokenRefreshState.TokenExpired:
                    case TokenRefreshState.TokenInvalid:
                        {
                            var refreshSucceeded = await strategy.RefreshTokenAsync(cancellationToken);

                            if (refreshSucceeded)
                            {
                                logger.LogInformation("{StrategyName}RefreshLoop: Token refreshed successfully for {ConnectionId}.", strategy.LoggingName, connectionId);
                                state = TokenRefreshState.HasToken;
                            }
                            else
                            {
                                logger.LogError("{StrategyName}RefreshLoop: Token refresh failed for {ConnectionId}.", strategy.LoggingName, connectionId);
                                var shouldContinue = await strategy.OnRefreshFailedAsync();
                                if (!shouldContinue)
                                {
                                    return;
                                }
                            }
                        }
                        break;
                }

                consecutiveErrors = 0;
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("{StrategyName}RefreshLoop: cancelled for {ConnectionId}", strategy.LoggingName, connectionId);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{StrategyName}RefreshLoop: Error during token refresh loop for {ConnectionId}", strategy.LoggingName, connectionId);
                consecutiveErrors++;
                if (consecutiveErrors >= 5)
                {
                    logger.LogError("{StrategyName}RefreshLoop: Too many consecutive errors for {ConnectionId}, exiting loop", strategy.LoggingName, connectionId);
                    return;
                }
                logger.LogInformation("{StrategyName}RefreshLoop: waiting 30 seconds before retrying for {ConnectionId}", strategy.LoggingName, connectionId);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                continue;
            }
        }
    }

    private async Task AuthRefreshLoopAsync(string connectionId, CancellationToken cancellationToken)
    {
        var session = sessionStore.Sessions[connectionId];
        var authService = session.AppServices.GetRequiredService<IAuthProviderService>();
        var authProvider = session.AppServices.GetRequiredService<IAuthProvider>();
        var authSession = authService.GetAuthProviderSession();

        var strategy = new MainAuthTokenRefreshStrategy(
            connectionId,
            authProvider,
            authService,
            authSession,
            sessionStore,
            contentBuilder,
            logger);

        await TokenRefreshLoopAsync(strategy, connectionId, cancellationToken);
    }

    private async Task OAuthTokenRefreshLoopAsync(string connectionId, OAuthProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var session = sessionStore.Sessions[connectionId];
            var registry = session.AppServices.GetService<IOAuthTokenHandlerRegistry>();
            if (registry == null)
            {
                logger.LogError("OAuthTokenRefreshLoop[{Provider}]: No OAuth token handler registry for {ConnectionId}, exiting loop.", provider, connectionId);
                return;
            }

            var handler = registry.GetHandler(provider);
            if (handler == null)
            {
                logger.LogError("OAuthTokenRefreshLoop[{Provider}]: No handler registered for {ConnectionId}, exiting loop.", provider, connectionId);
                return;
            }

            var authService = session.AppServices.GetRequiredService<IAuthProviderService>();
            var authSession = authService.GetAuthProviderSession();

            // Get the provider's session
            if (!authSession.OAuthProviderSessions.TryGetValue(provider, out var providerSession))
            {
                logger.LogError("OAuthTokenRefreshLoop[{Provider}]: No session found for {ConnectionId}, exiting loop.", provider, connectionId);
                return;
            }

            var client = session.AppServices.GetRequiredService<IClientProvider>();
            var oauthLogger = session.AppServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger<OAuthTokenService>();

            // Create a service instance for this provider
            var oauthTokenService = new OAuthTokenService(
                provider,
                handler,
                providerSession,
                authSession,
                client,
                sessionStore,
                oauthLogger);

            var strategy = new OAuthTokenRefreshStrategy(connectionId, oauthTokenService, handler, logger);

            await TokenRefreshLoopAsync(strategy, connectionId, cancellationToken);
        }
        finally
        {
            // Clean up: remove provider from active set when loop exits
            if (_activeOAuthRefreshLoops.TryGetValue(connectionId, out var activeProviders))
            {
                lock (activeProviders)
                {
                    activeProviders.Remove(provider);
                }
            }

            // Clean up: dispose the cancellation token source
            if (_oauthRefreshCancellations.TryGetValue(connectionId, out var cancellations))
            {
                if (cancellations.TryRemove(provider, out var cts))
                {
                    cts.Dispose();
                }
            }
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

    public void StreamSubscribe(string streamId)
    {
        logger.LogDebug("StreamSubscribe: {StreamId}", streamId);
        StreamRegistry.NotifySubscribed(streamId);
    }

    public async Task Navigate(string? appId, ClientExtensions.HistoryState? state)
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

            await navigateSignal.Send(new NavigateArgs(appId, TabId: state?.TabId, HistoryOp: HistoryOp.Pop));

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
    private volatile bool _disposed;

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
        if (_disposed) return;

        if (!_channel.Writer.TryWrite((method, data)))
        {
            // Channel full or completed — try async write, but guard against disposal race
            if (_disposed) return;
            try
            {
                _ = _channel.Writer.WriteAsync((method, data), _cts.Token);
            }
            catch (ObjectDisposedException) { }
        }
    }

    public void Dispose()
    {
        _disposed = true;

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