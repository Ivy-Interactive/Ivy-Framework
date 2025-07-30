using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Exceptions;
using Ivy.DataTables;
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
    ILogger<AppHub> logger,
    IQueryableRegistry queryableRegistry
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
        var appServices = new ServiceCollection();

        var httpContext = Context.GetHttpContext()!;
        var appId = GetAppId(server, httpContext);

        var isAuthProtected = server.AuthProviderType != null;
        AuthToken? authToken = null, oldAuthToken = null;
        if (isAuthProtected)
        {
            var authProvider = server.Services.BuildServiceProvider().GetService<IAuthProvider>() ?? throw new Exception("IAuthProvider not found");
            var jwt = httpContext.Request.Cookies["jwt"].NullIfEmpty();

            try
            {
                oldAuthToken = authToken = jwt != null
                    ? JsonSerializer.Deserialize<AuthToken>(jwt)
                    : null;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to deserialize AuthToken from JWT.");
            }

            if (string.IsNullOrEmpty(authToken?.Jwt))
            {
                appId = AppIds.Auth;
            }
            else
            {
                authToken = await authProvider.RefreshJwtAsync(authToken);
                if (string.IsNullOrEmpty(authToken?.Jwt))
                {
                    appId = AppIds.Auth;
                }
                else
                {
                    var validJwt = await authProvider.ValidateJwtAsync(authToken.Jwt);
                    if (!validJwt)
                    {
                        appId = AppIds.Auth;
                    }
                }
            }
            appServices.AddSingleton<IAuthService>(s => new AuthService(authProvider, authToken));
        }

        var appArgs = GetAppArgs(Context.ConnectionId, appId, httpContext);
        var appDescriptor = server.GetApp(appId);

        logger.LogInformation($"Connected: {Context.ConnectionId} [{appId}]");

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
        appServices.AddSingleton(typeof(IDataTableService), new DataTableConnectionService(
            queryableRegistry,
            server.Args,
            Context.ConnectionId));
        appServices.AddSingleton(typeof(IClientProvider), clientProvider);
        appServices.AddSingleton(appDescriptor);
        appServices.AddSingleton(appArgs);

        appServices.AddTransient<IWebhookRegistry, WebhookController>();
        appServices.AddTransient<SignalRouter>(_ => new SignalRouter(sessionStore));

        if (authToken != oldAuthToken)
        {
            clientProvider.SetJwt(authToken);
        }

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
            LastInteraction = DateTime.UtcNow
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
                Console.WriteLine(e);
            }
        }

        appState.TrackDisposable(widgetTree.Subscribe(OnWidgetTreeChanged));

        sessionStore.Sessions[Context.ConnectionId] = appState;

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

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (sessionStore.Sessions.TryRemove(Context.ConnectionId, out var appState))
        {
            appState.Dispose();
        }

        return base.OnDisconnectedAsync(exception);
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

    public async Task Event(string eventName, string widgetId, JsonArray? args)
    {
        logger.LogInformation($"Event: {eventName} {widgetId} {args}");
        if (!sessionStore.Sessions.TryGetValue(Context.ConnectionId, out var appSession))
        {
            logger.LogWarning($"Event: {eventName} {widgetId} [AppSession Not Found]");
            return;
        }

        try
        {
            if (server.AuthProviderType != null)
            {
                if (appSession.AppId != AppIds.Auth)
                {
                    var authProvider = server.Services.BuildServiceProvider()
                                           .GetService<IAuthProvider>() ??
                                       throw new Exception("IAuthProvider not found");

                    var jwt = Context.GetHttpContext()!.Request.Cookies["jwt"].NullIfEmpty();

                    // TODO: handle deserialization errors
                    var authToken = jwt != null
                        ? JsonSerializer.Deserialize<AuthToken>(jwt)
                        : null;

                    if (string.IsNullOrEmpty(authToken?.Jwt) || !await authProvider.ValidateJwtAsync(authToken.Jwt))
                    {
                        logger.LogWarning(
                            "Invalid JWT for event from {ConnectionId}. Aborting.",
                            Context.ConnectionId);
                        Context.Abort();
                        return;
                    }
                }
            }

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
}

public class ClientSender(IClientNotifier clientNotifier, string connectionId) : IClientSender
{
    public void Send(string method, object? data)
    {
        _ = clientNotifier.NotifyClientAsync(connectionId, method, data);
    }
}

public class ClientProvider(IClientSender sender) : IClientProvider
{
    public IClientSender Sender { get; set; } = sender;
}
