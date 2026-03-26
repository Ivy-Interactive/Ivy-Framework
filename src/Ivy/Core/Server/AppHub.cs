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
    private static readonly HashSet<string> ReservedQueryParams = new(StringComparer.OrdinalIgnoreCase)
    {
        "appId", "machineId", "parentId", "shell", "appArgs", "oauthLogin", "id"
    };

    private AppContext GetAppArgs(string connectionId, string machineId, string appId, string? navigationAppId, HttpContext httpContext, string requestScheme)
    {
        string? appArgs = null;

        // First check for explicit appArgs parameter (takes precedence)
        if (httpContext.Request.Query.TryGetValue("appArgs", out var appArgsParam))
        {
            appArgs = appArgsParam.ToString().NullIfEmpty();
        }

        // If no explicit appArgs, build JSON from individual query parameters
        if (appArgs == null && httpContext.Request.Query.Count > 0)
        {
            var argsDict = new Dictionary<string, string>();

            foreach (var kvp in httpContext.Request.Query)
            {
                if (ReservedQueryParams.Contains(kvp.Key))
                    continue;

                var value = kvp.Value.FirstOrDefault();
                if (value != null)
                {
                    argsDict[kvp.Key] = value;
                }
            }

            if (argsDict.Count > 0)
            {
                appArgs = System.Text.Json.JsonSerializer.Serialize(argsDict, JsonHelper.DefaultOptions);
            }
        }

        // Get base path from X-Forwarded-Prefix header (for reverse proxy), or fall back to server.Args
        var basePath = server.Args?.BasePath;
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var forwardedPrefix) && !string.IsNullOrEmpty(forwardedPrefix.ToString()))
        {
            basePath = forwardedPrefix.ToString().Trim('/');
        }

        var requestHost = httpContext.Request.Host.Value!;
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-Host", out var forwardedHost) && !string.IsNullOrEmpty(forwardedHost.ToString()))
            requestHost = forwardedHost.ToString();

        return new AppContext(connectionId, machineId, appId, navigationAppId, appArgs ?? server.Args?.Args, requestScheme, requestHost, basePath);
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
            appServices.AddSingleton(tunneledHttpHandler);

            var request = httpContext.Request;
            var requestScheme = request.Scheme;
            if (request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto))
            {
                requestScheme = forwardedProto.ToString();
            }

            var machineId = AppRouter.GetMachineId(httpContext);

            // Resolve route before auth so we can avoid reload loop on error page (skip LogoutAsync) and avoid overriding to Auth app
            var appRouter = new AppRouter(server);
            var routeResult = appRouter.Resolve(httpContext);
            var isErrorAppRequest = routeResult.AppId == AppIds.ErrorNotFound || routeResult.NavigationAppId == AppIds.ErrorNotFound;

            if (server.AuthProviderType != null)
            {
                var authProvider = server.ServiceProvider!.GetService<IAuthProvider>() ?? throw new Exception("IAuthProvider not found");
#if DEBUG
                authProvider = new CheckedAuthProvider(authProvider);
#endif

                var authSession = AuthHelper.GetAuthSession(httpContext, tunneledHttpHandler);
                var authServiceLogger = server.ServiceProvider?.GetService<ILoggerFactory>()?.CreateLogger<AuthService>();
                var authService = new AuthService(authProvider, authSession, clientProvider, sessionStore, machineId, server.ServiceProvider, authServiceLogger);

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

                // Don't call LogoutAsync when user is on the error page: nothing to clear and it would send SetAuthCookies(reloadPage: true), causing a reload loop
                if (authSession.AuthToken == null && parentId != null && !isErrorAppRequest)
                {
                    await authService.LogoutAsync(Context.ConnectionAborted);
                }
            }

            // Reuse routeResult and isErrorAppRequest from above
            // Override to Auth app if authentication failed (unless they requested the error page)
            if (server.AuthProviderType != null && !isErrorAppRequest)
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

            if (routeResult.AppDescriptor.Title is { } title && routeResult.AppId != AppIds.AppShell && parentId == null)
            {
                clientProvider.SetTitle(title, server.Args.Metadata.Title);
            }

            appServices.AddSingleton(routeResult.AppRepository);

            var appArgs = GetAppArgs(Context.ConnectionId, machineId, routeResult.AppId, routeResult.NavigationAppId, httpContext, requestScheme);
            if (routeResult.ArgsJson != null)
            {
                appArgs = new AppContext(Context.ConnectionId, machineId, routeResult.AppId, routeResult.NavigationAppId, routeResult.ArgsJson, requestScheme, appArgs.Host, appArgs.BasePath);
            }

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

                if (routeResult.AppId != AppIds.AppShell && !isNotFoundPage)
                {
                    var navigateArgs = new NavigateArgs(routeResult.AppId, AppShell: routeResult.ShowAppShell);
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

            // Trigger reload for other tabs on this machine after OAuth login
            if (parentId == null &&
                httpContext.Request.Query.ContainsKey("oauthLogin") &&
                serviceProvider.GetService<IAuthService>()?.GetCurrentToken() != null)
            {
                var oauthConnectionId = Context.ConnectionId;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100); // Small delay to ensure cookies are processed
                    AuthController.TriggerMachineReload(sessionStore, machineId, oauthConnectionId);
                });
            }

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
                var tree = new WidgetTree(new ExceptionErrorView(e), contentBuilder, serviceProvider);
                await tree.BuildAsync();
                await Clients.Caller.SendAsync("Refresh", new
                {
                    Widgets = tree.GetWidgets().Serialize(),
                    ExternalWidgets = (object?)null
                }, cancellationToken: connectionAborted);
            }

            if (server.AuthProviderType != null && routeResult.AppId != AppIds.Auth && routeResult.AppId != AppIds.AppShell)
            {
                _ = Task.Run(() => AuthRefreshLoopAsync(connectionId, connectionAborted), connectionAborted);

                // Start a refresh loop for each brokered auth session
                var authService = appState.AppServices.GetService<IAuthService>();
                if (authService != null)
                {
                    var authSession = authService.GetAuthSession();
                    var brokeredSessions = authSession.BrokeredSessions.Keys.ToList();

                    // Track active brokered refresh loops and their cancellation tokens on the session
                    var activeProviders = new HashSet<string>();
                    var cancellations = new ConcurrentDictionary<string, CancellationTokenSource>();
                    appState.ActiveBrokeredRefreshLoops = activeProviders;
                    appState.BrokeredRefreshCancellations = cancellations;

                    void AddProvider(string provider)
                    {
                        lock (activeProviders)
                        {
                            if (activeProviders.Add(provider))
                            {
                                var cts = CancellationTokenSource.CreateLinkedTokenSource(connectionAborted);
                                cancellations[provider] = cts;
                                _ = Task.Run(() => BrokeredTokenRefreshLoopAsync(connectionId, provider, cts.Token), connectionAborted);
                            }
                        }
                    }

                    foreach (var brokeredSession in brokeredSessions)
                    {
                        AddProvider(brokeredSession);
                    }

                    // Subscribe to new brokered auth sessions being added
                    Action<string> addedHandler = provider =>
                    {
                        // Check if connection is still active
                        if (!sessionStore.Sessions.ContainsKey(connectionId))
                        {
                            return;
                        }

                        AddProvider(provider);
                    };

                    // Subscribe to brokered auth sessions being removed
                    Action<string> removedHandler = provider =>
                    {
                        logger.LogInformation("Cancelling brokered token refresh loop for removed provider {Provider} on connection {ConnectionId}", provider, connectionId);

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

                    appState.BrokeredTokenAddedHandler = addedHandler;
                    appState.BrokeredTokenRemovedHandler = removedHandler;
                    authSession.BrokeredSessionAdded += addedHandler;
                    authSession.BrokeredSessionRemoved += removedHandler;
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
            if (exception is OperationCanceledException)
            {
                logger.LogDebug("Client {ConnectionId} timed out", Context.ConnectionId);
            }
            else if (exception != null)
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
                    // Clean up brokered session event subscriptions
                    if (appState.BrokeredTokenAddedHandler != null || appState.BrokeredTokenRemovedHandler != null)
                    {
                        var authService = appState.AppServices.GetService<IAuthService>();
                        if (authService != null)
                        {
                            var authSession = authService.GetAuthSession();
                            if (appState.BrokeredTokenAddedHandler != null)
                            {
                                authSession.BrokeredSessionAdded -= appState.BrokeredTokenAddedHandler;
                            }
                            if (appState.BrokeredTokenRemovedHandler != null)
                            {
                                authSession.BrokeredSessionRemoved -= appState.BrokeredTokenRemovedHandler;
                            }
                        }
                    }

                    // Cancel and dispose all brokered token refresh loop cancellation tokens
                    if (appState.BrokeredRefreshCancellations != null)
                    {
                        foreach (var kvp in appState.BrokeredRefreshCancellations)
                        {
                            try
                            {
                                kvp.Value.Cancel();
                                kvp.Value.Dispose();
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Error cancelling brokered token refresh loop for provider {Provider} on connection {ConnectionId}", kvp.Key, Context.ConnectionId);
                            }
                        }
                    }

                    // Dispose app state (stops EventDispatchQueue, cleans up widget tree)
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
        DateTimeOffset? lastRefreshTime = null;

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

                                // Use NotBefore from token, or fall back to lastRefreshTime if we recently refreshed
                                var effectiveNotBefore = lifetime?.NotBefore ?? lastRefreshTime;

                                TimeSpan renewalMargin;
                                if (lifetime?.Expires != null && effectiveNotBefore != null &&
                                    lifetime.Expires - effectiveNotBefore is { } duration &&
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
                                lastRefreshTime = DateTimeOffset.UtcNow;
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
        var authService = session.AppServices.GetRequiredService<IAuthService>();
        var authSession = authService.GetAuthSession();

        var strategy = new MainAuthTokenRefreshStrategy(
            authService,
            authSession,
            session,
            contentBuilder,
            logger);

        await TokenRefreshLoopAsync(strategy, connectionId, cancellationToken);
    }

    private async Task BrokeredTokenRefreshLoopAsync(string connectionId, string provider, CancellationToken cancellationToken)
    {
        if (!sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            logger.LogWarning("BrokeredTokenRefreshLoop[{Provider}]: Session not found for {ConnectionId}, exiting loop.", provider, connectionId);
            return;
        }

        try
        {
            var handler = server.ServiceProvider!.GetKeyedService<IAuthTokenHandler>(provider);
            if (handler == null)
            {
                logger.LogError("BrokeredTokenRefreshLoop[{Provider}]: No handler registered for {ConnectionId}, exiting loop.", provider, connectionId);
                return;
            }

#if DEBUG
            handler = new Ivy.Core.Auth.CheckedAuthTokenHandler(handler);
#endif

            var authService = session.AppServices.GetRequiredService<IAuthService>();
            var authSession = authService.GetAuthSession();

            // Get the provider's session
            if (!authSession.BrokeredSessions.TryGetValue(provider, out var providerSession))
            {
                logger.LogError("BrokeredTokenRefreshLoop[{Provider}]: No session found for {ConnectionId}, exiting loop.", provider, connectionId);
                return;
            }

            // Initialize the token handler using AppContext (not Hub.Context which can be disposed)
            var appContext = session.AppServices.GetService<AppContext>();
            if (appContext != null)
            {
                await handler.InitializeAsync(providerSession, appContext.Scheme, appContext.Host, cancellationToken);
            }

            var client = session.AppServices.GetRequiredService<IClientProvider>();
            var brokeredLogger = session.AppServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger<AuthTokenHandlerService>();

            // Create a service instance for this provider
            var tokenService = new AuthTokenHandlerService(
                handler,
                providerSession,
                client,
                sessionStore,
                session.MachineId,
                brokeredLogger);

            var strategy = new BrokeredTokenRefreshStrategy(
                connectionId,
                provider,
                tokenService,
                authSession,
                client,
                authService,
                sessionStore,
                contentBuilder,
                brokeredLogger);

            await TokenRefreshLoopAsync(strategy, connectionId, cancellationToken);
        }
        finally
        {
            // Clean up: remove provider from active set when loop exits
            if (session.ActiveBrokeredRefreshLoops != null)
            {
                lock (session.ActiveBrokeredRefreshLoops)
                {
                    session.ActiveBrokeredRefreshLoops.Remove(provider);
                }
            }

            // Clean up: dispose the cancellation token source
            if (session.BrokeredRefreshCancellations?.TryRemove(provider, out var cts) == true)
            {
                cts.Dispose();
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
        logger.LogDebug("Event received: {EventName} {WidgetId} ConnectionId={ConnectionId}", eventName, widgetId, Context.ConnectionId);
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogDebug("Event: {EventName} {WidgetId} [AppSession Not Found] ConnectionId={ConnectionId}", eventName, widgetId, Context.ConnectionId);
            return Task.CompletedTask;
        }

        if (appSession.EventQueue == null)
        {
            logger.LogWarning("Event: {EventName} {WidgetId} [EventQueue is null!]", eventName, widgetId);
            return Task.CompletedTask;
        }

        // Enqueue async event handling to avoid tying up ThreadPool workers
        appSession.EventQueue.Enqueue(async () =>
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

        // Find the AppShell session for this connection
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogWarning("Navigate: {ConnectionId} [{AppId}] [AppSession not found]", Context.ConnectionId, appId);
            return;
        }

        var appShellSession = sessionStore.FindAppShell(appSession);
        if (appShellSession == null)
        {
            logger.LogWarning("Navigate: {ConnectionId} [{AppId}] [AppShell session not found]", Context.ConnectionId, appId);
            return;
        }

        try
        {
            var navigateSignal = (NavigateSignal)appShellSession.Signals.GetOrAdd(
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