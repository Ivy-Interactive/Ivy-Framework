using System.Diagnostics;
using System.Reflection;
using System.Text;
using Ivy.Core;
using Ivy.Core.Apps;
using Ivy.Core.Auth;
using Ivy.Core.ExternalWidgets;
using Ivy.Core.Server;
using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
using Ivy.Core.Server.Middleware;
using Ivy.Core.Server.Formatters;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; //do not remove - used in RELEASE
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy;

public record ServerMetadata
{
    public string? Title { get; set; } = null;
    public string? Description { get; set; } = null;
    public string? GitHubUrl { get; set; } = null;
    public string? OgImage { get; set; } = null;
    public string? OgSiteName { get; set; } = null;
    public string? OgType { get; set; } = "website";
    public string? OgLocale { get; set; } = "en_US";
    public string? TwitterCard { get; set; } = "summary_large_image";
}

public record ServerArgs
{
    public const int DefaultPort = 5010;
    public int Port { get; set; } = DefaultPort;
    public bool Verbose { get; set; } = false;
    public bool IKillForThisPort { get; set; } = false;
    public bool Browse { get; set; } = false;
    public string? Args { get; set; } = null;
    public string? DefaultAppId { get; set; } = null;
    public bool Silent { get; set; } = false;
    public bool Describe { get; set; } = false;
    public string? DescribeConnection { get; set; } = null;
    public string? TestConnection { get; set; } = null;
    public ServerMetadata Metadata { get; set; } = new();
    public Assembly? AssetAssembly { get; set; } = null;
    public bool EnableDevTools { get; set; } = false;
    public bool DangerouslyAllowLocalFiles { get; set; } = false;
#if DEBUG
    public bool FindAvailablePort { get; set; } = true;
#else
    public bool FindAvailablePort { get; set; } = false;
#endif
    public string? Host { get; set; } = null;

    /// <summary>
    /// Base path for the application when running behind a reverse proxy (e.g., "/myapp").
    /// </summary>
    public string? BasePath { get; set; } = null;

    /// <summary>
    /// True when the process is running a CLI-only command (--describe, --describe-connection, --test-connection)
    /// that needs DI but should not bind a real port.
    /// </summary>
    public bool IsCliCommand => Describe || DescribeConnection != null || TestConnection != null;
}

public class Server
{
    public IReadOnlySet<string> ReservedPaths => _reservedPaths;
    public string? DefaultAppId { get; private set; }
    public AppRepository AppRepository { get; } = new();
    public NavigationBeaconRegistry NavigationBeaconRegistry { get; } = new();
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IConfiguration Configuration { get; private set; } = ServerUtils.GetConfiguration();
    public Type? AuthProviderType { get; private set; } = null;
    public ServerArgs Args => _args;
    public static Action<CookieOptions>? ConfigureAuthCookieOptions { get; set; }
    private IContentBuilder? _contentBuilder;
    private bool _useHotReload;
    private bool _useHttpRedirection;
    internal IServiceProvider? ServiceProvider;
    private readonly List<Action<WebApplicationBuilder>> _builderMods = new();
    private readonly List<Action<WebApplication>> _appMods = new();
    private HashSet<string> _reservedPaths = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _fluentApiReservedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IHtmlFilter> _customHtmlFilters = new();
    private Action<HtmlPipeline>? _pipelineConfigurator;
    private ManifestOptions? _manifestOptions;
    private ServerArgs _args;

    public Server(ServerArgs? args = null)
    {
        _args = args ?? ServerUtils.GetArgs();
        if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out int parsedPort))
        {
            _args = _args with { Port = parsedPort };
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("VERBOSE"), out bool parsedVerbose))
        {
            _args = _args with { Verbose = parsedVerbose };
        }

        if (_args.Host == null && Environment.GetEnvironmentVariable("HOST") is { } host)
        {
            _args = _args with { Host = host };
        }

        if (_args.BasePath == null && Environment.GetEnvironmentVariable("BASE_PATH") is { } basePath)
        {
            _args = _args with { BasePath = basePath };
        }

        _args = _args with
        {
            AssetAssembly = _args.AssetAssembly ?? Assembly.GetCallingAssembly(),
        };

        Services.AddSingleton(_args);
        // capture the latest Configuration value at resolution time in case it gets replaced by UseConfiguration()
        Services.AddSingleton(_ => Configuration);

        AddDefaultApps();
    }

    private void AddDefaultApps()
    {
        UseErrorNotFound<ErrorApp>();
    }

    public Server(FuncViewBuilder viewFactory) : this()
    {
        AddApp(new AppDescriptor
        {
            Id = AppIds.Default,
            Title = "Default",
            ViewFunc = viewFactory,
            Group = ["Apps"],
            IsVisible = true
        });
        DefaultAppId = AppIds.Default;
    }

    public void AddApp<T>(bool isDefault = false)
    {
        AddApp(typeof(T), isDefault);
    }

    public void AddApp(Type appType, bool isDefault = false)
    {
        AppRepository.AddFactory(() => [AppHelpers.GetApp(appType)]);
        if (isDefault)
            DefaultAppId = AppHelpers.GetApp(appType)?.Id;
    }

    public void AddApp(AppDescriptor appDescriptor)
    {
        AppRepository.AddFactory(() => [appDescriptor]);
    }

    public void AddAppsFromAssembly(Assembly? assembly = null)
    {
        AppRepository.AddFactory(() => AppHelpers.GetApps(assembly));
    }

    public void AddConnectionsFromAssembly(Assembly? assembly = null)
    {
        assembly ??= Assembly.GetEntryAssembly();

        var connections = assembly!.GetLoadableTypes()
            .Where(t => t.IsClass && typeof(IConnection).IsAssignableFrom(t));

        foreach (var type in connections)
        {
            var connection = (IConnection)Activator.CreateInstance(type)!;
            connection.RegisterServices(this);
        }
    }

    public AppDescriptor GetApp(string id)
    {
        return AppRepository.GetAppOrDefault(id);
    }

    public Server UseContentBuilder(IContentBuilder contentBuilder)
    {
        _contentBuilder = contentBuilder;
        return this;
    }

    public Server UseHotReload()
    {
        _useHotReload = true;
        return this;
    }

    public Server UseHttpRedirection()
    {
        _useHttpRedirection = true;
        return this;
    }

    public Server UseCulture(string cultureName)
    {
        var culture = new System.Globalization.CultureInfo(cultureName);
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        return this;
    }

    public Server UseConfiguration(IConfiguration configuration)
    {
        Configuration = configuration;
        return this;
    }

    public Server UseConfiguration(Action<IConfigurationBuilder> configure)
    {
        Configuration = ServerUtils.GetConfiguration(configure);
        return this;
    }

    public Server UseAppShell(AppShellSettings settings)
    {
        return UseAppShell(() => new DefaultSidebarAppShell(settings));
    }

    public Server UseAppShell<T>() where T : ViewBase, new()
    {
        return UseAppShell((() => (ViewBase)Activator.CreateInstance(typeof(T))!));
    }

    public Server UseAppShell(Func<ViewBase>? viewFactory = null)
    {
        AddApp(new AppDescriptor
        {
            Id = AppIds.AppShell,
            Title = "AppShell",
            ViewFactory = viewFactory ?? (() => new DefaultSidebarAppShell(AppShellSettings.Default())),
            Group = [],
            IsVisible = false
        });
        DefaultAppId = AppIds.AppShell;
        return this;
    }

    public Server UseAuth<T>(Action<T>? config = null, Func<ViewBase>? viewFactory = null) where T : class, IAuthProvider
    {
        Services.AddSingleton<T>();
        Services.AddSingleton<IAuthProvider, T>(s =>
        {
            T provider = s.GetRequiredService<T>();
            config?.Invoke(provider);
            return provider;
        });

        DiscoverAndRegisterOAuthTokenHandlers();

        AddApp(new AppDescriptor
        {
            Id = AppIds.Auth,
            Title = "Auth",
            ViewFactory = viewFactory ?? (() => new DefaultAuthApp()),
            Group = [],
            IsVisible = false
        });
        AuthProviderType = typeof(T);
        return this;
    }

    public Server RegisterAuthTokenHandler<T>(string provider) where T : class, IAuthTokenHandler
    {
        Services.AddKeyedSingleton<IAuthTokenHandler, T>(provider);
        return this;
    }

    private void DiscoverAndRegisterOAuthTokenHandlers()
    {
        try
        {
            // Load all "Ivy.Auth.*" assemblies eagerly to ensure their handlers are registered in the DI container
            var assemblyDirectory = System.AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(assemblyDirectory))
            {
                var authAssemblyFiles = Directory.GetFiles(assemblyDirectory, "Ivy.Auth.*.dll");

                foreach (var assemblyFile in authAssemblyFiles)
                {
                    try
                    {
                        Assembly.LoadFrom(assemblyFile);
                    }
                    catch
                    {
                        // Continue if we can't load an assembly
                    }
                }
            }

            // Now get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Skip system assemblies for performance
                    var assemblyName = assembly.GetName().Name ?? "";
                    if (assemblyName.StartsWith("System.") ||
                        assemblyName.StartsWith("Microsoft.") ||
                        assemblyName == "netstandard" ||
                        assemblyName == "mscorlib")
                    {
                        continue;
                    }

                    // Find all types with OAuthTokenHandlerAttribute
                    var handlerTypes = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && typeof(IAuthTokenHandler).IsAssignableFrom(t))
                        .Where(t => t.GetCustomAttribute<OAuthTokenHandlerAttribute>() != null)
                        .ToList();

                    foreach (var handlerType in handlerTypes)
                    {
                        var attribute = handlerType.GetCustomAttribute<OAuthTokenHandlerAttribute>();
                        if (attribute == null)
                            continue;

                        try
                        {
                            Services.AddKeyedSingleton(typeof(IAuthTokenHandler), attribute.Provider, handlerType);
                        }
                        catch
                        {
                            // Continue if we can't register a handler
                        }
                    }
                }
                catch
                {
                    // Continue loading types from an assembly fails
                }
            }
        }
        catch
        {
            // Continue if discovery completely fails, just continue with no handlers
        }
    }

    public Server UseDefaultApp(Type appType)
    {
        DefaultAppId = AppHelpers.GetApp(appType).Id;
        return this;
    }

    public Server UseErrorNotFound<T>() where T : ViewBase, new()
    {
        return UseErrorNotFound((() => (ViewBase)Activator.CreateInstance(typeof(T))!));
    }

    public Server UseErrorNotFound(Func<ViewBase>? viewFactory = null)
    {
        AddApp(new AppDescriptor
        {
            Id = AppIds.ErrorNotFound,
            Title = "App Not Found",
            ViewFactory = viewFactory,
            Group = [],
            IsVisible = false
        });
        return this;
    }

    public Server UseWebApplicationBuilder(Action<WebApplicationBuilder> modify)
    {
        _builderMods.Add(modify);
        return this;
    }

    public Server UseWebApplication(Action<WebApplication> modify)
    {
        _appMods.Add(modify);
        return this;
    }

    public Server ReservePaths(params string[] paths)
    {
        if (paths != null)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                var normalizedPath = path.StartsWith('/') ? path : "/" + path;
                _fluentApiReservedPaths.Add(normalizedPath);
                _reservedPaths.Add(normalizedPath);
            }
        }
        return this;
    }

    public Server SetMetaTitle(string title)
    {
        _args.Metadata.Title = title;
        return this;
    }

    public Server SetMetaDescription(string description)
    {
        _args.Metadata.Description = description;
        return this;
    }

    public Server SetMetaGitHubUrl(string url)
    {
        _args.Metadata.GitHubUrl = url;
        return this;
    }

    public Server UseTheme(Theme theme)
    {
        var themeService = new ThemeService();
        themeService.SetTheme(theme);

        var themeType = theme.GetType();
        if (themeType != typeof(Theme))
        {
            themeService.SetThemeFactory(() => (Theme)Activator.CreateInstance(themeType)!);
        }

        Services.AddSingleton<IThemeService>(themeService);
        return this;
    }

    public Server UseTheme(Func<Theme> themeFactory)
    {
        var theme = themeFactory();
        var themeService = new ThemeService();
        themeService.SetTheme(theme);
        themeService.SetThemeFactory(themeFactory);

        Services.AddSingleton<IThemeService>(themeService);
        return this;
    }

    public Server UseTheme(Action<Theme> configureTheme)
    {
        var theme = new Theme();
        configureTheme(theme);
        var themeService = new ThemeService();
        themeService.SetTheme(theme);

        themeService.SetThemeFactory(() =>
        {
            var t = new Theme();
            configureTheme(t);
            return t;
        });

        Services.AddSingleton<IThemeService>(themeService);
        return this;
    }

    public Server UseManifest(Action<ManifestOptions>? configure = null)
    {
        _manifestOptions = new ManifestOptions();
        configure?.Invoke(_manifestOptions);
        Services.AddSingleton(_manifestOptions);
        return this;
    }

    public Server DangerouslyAllowLocalFiles()
    {
        _args = _args with { DangerouslyAllowLocalFiles = true };
        return this;
    }

    public Server UseHtmlFilter(IHtmlFilter filter)
    {
        _customHtmlFilters.Add(filter);
        return this;
    }

    public Server UseHtmlPipeline(Action<HtmlPipeline> configure)
    {
        _pipelineConfigurator = configure;
        return this;
    }

    internal IReadOnlyList<IHtmlFilter> GetCustomFilters() => _customHtmlFilters;

    internal Action<HtmlPipeline>? GetPipelineConfigurator() => _pipelineConfigurator;


    internal ManifestOptions? GetManifestOptions() => _manifestOptions;

    internal WebApplication? BuildWebApplication(CancellationTokenSource? cts = null)
    {
        var sessionStore = new AppSessionStore();
        return BuildWebApplication(sessionStore, cts);
    }

    internal WebApplication? BuildWebApplication(AppSessionStore sessionStore, CancellationTokenSource? cts = null)
    {
        if (!string.IsNullOrEmpty(_args.DefaultAppId))
        {
            DefaultAppId = _args.DefaultAppId;
        }

        // Initialize external widget registry by scanning loaded assemblies
        ExternalWidgetRegistry.Instance.Initialize();

        // Register navigation beacons from all loaded assemblies
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.GetLoadableTypes().Any());
        foreach (var assembly in loadedAssemblies)
        {
            try
            {
                AppHelpers.RegisterBeacons(assembly, NavigationBeaconRegistry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to register beacons from {assembly.GetName().Name}: {ex.Message}");
            }
        }

        // Ensure sufficient ThreadPool workers to avoid heartbeat warnings under bursty loads
        try
        {
            ThreadPool.GetMinThreads(out var workerMin, out var ioMin);
            var target = Math.Max(workerMin, Environment.ProcessorCount * 16);
            var targetIo = Math.Max(ioMin, Environment.ProcessorCount * 16);
            ThreadPool.SetMinThreads(target, targetIo);
        }
        catch { /* best-effort */ }

        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddConfiguration(Configuration);

        foreach (var mod in _builderMods)
        {
            mod(builder);
        }

        // CLI-only commands need DI but never call app.StartAsync(),
        // so use port 0 to avoid conflicts with a running instance.
        // Bind to localhost for local dev (avoids Windows Firewall prompt),
        // but use wildcard in containers so health probes can reach the app.
        // On Sliplane or other hosted environments, we usually have PORT set and need to listen on 0.0.0.0.
        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var hasPortEnv = Environment.GetEnvironmentVariable("PORT") != null;
        var host = _args.Host ?? (isContainer || hasPortEnv ? "*" : "localhost");
        var bindUrl = _args.IsCliCommand ? "http://localhost:0" : $"http://{host}:{_args.Port}";
        builder.WebHost.UseUrls(bindUrl);

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = _args.Verbose;
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(3);
            options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            options.MaximumReceiveMessageSize = 1048576; // 1MB
        }).AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver();
        }).AddMessagePackProtocol(options =>
        {
            options.SerializerOptions = MessagePackSerializerOptions.Standard.WithResolver(
                CompositeResolver.Create(
                    new IMessagePackFormatter[] {
                        new JsonNodeMessagePackFormatter(),
                        new JsonObjectMessagePackFormatter(),
                        new JsonArrayMessagePackFormatter(),
                        new JsonValueMessagePackFormatter()
                    },
                    new IFormatterResolver[] {
                        JsonNodeResolver.Instance,
                        ContractlessStandardResolver.Instance
                    }
                )
            );
        });
        builder.Services.AddSingleton(this);
        builder.Services.AddSingleton<IClientNotifier, ClientNotifier>();
        builder.Services.AddControllers()
            .AddApplicationPart(Assembly.Load("Ivy"))
            .AddControllersAsServices();
        builder.Services.AddGrpc();
        builder.Services.AddSingleton<IQueryableRegistry, QueryableRegistry>();
        builder.Services.AddSingleton(_contentBuilder ?? new DefaultContentBuilder());
        builder.Services.AddSingleton(sessionStore);
        builder.Services.AddSingleton<IOAuthCallbackRegistry, OAuthCallbackRegistry>();
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddHealthChecks();
        builder.Services.AddQueryManager();

        // Register theme service if not already registered
        if (Services.All(s => s.ServiceType != typeof(IThemeService)))
        {
            Services.AddSingleton<IThemeService, ThemeService>();
        }

        // Register NavigationBeaconRegistry as a singleton service
        builder.Services.AddSingleton<INavigationBeaconRegistry>(NavigationBeaconRegistry);

        // Register all services from this server's Services collection
        foreach (var service in Services)
        {
            builder.Services.Add(service);
        }

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Required for SignalR
            });
        });

        if (_useHttpRedirection)
        {
            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 443;
            });
        }

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Logging.SetMinimumLevel(!_args.Verbose ? LogLevel.Warning : LogLevel.Information);

        // Suppress hosting startup errors when not verbose (we handle IOException with a friendly message)
        if (!_args.Verbose)
        {
            builder.Logging.AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.None);
        }

        var app = builder.Build();
        ServiceProvider = app.Services;

        // Update reserved paths with discovered controller routes before reloading apps
        UpdateReservedPaths(app);
        AppRepository.ClearInvalidAppIds();
        AppRepository.Reload(_reservedPaths);
        if (AppRepository.InvalidAppIds.Count > 0)
        {
            Console.WriteLine($@"[CRITICAL] Failed to start Ivy server due to {AppRepository.InvalidAppIds.Count} invalid app ID(s).");
            return null;
        }

        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (errorFeature != null)
                {
                    var ex = errorFeature.Error;

                    var logger = app.Services.GetRequiredService<ILogger<Server>>();
                    logger.LogError(ex, "An unhandled exception occurred.");

                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        detail = ex.StackTrace
                    }, Ivy.Core.Helpers.JsonHelper.DefaultOptions);
                    await context.Response.WriteAsync(result);
                }
            });
        });

        if (_useHttpRedirection)
        {
            app.UseHttpsRedirection();
        }

        var logger2 = _args.Verbose ? app.Services.GetRequiredService<ILogger<Server>>() : new NullLogger<Server>();

        // Configure ForwardedHeaders middleware to process X-Forwarded-* headers from reverse proxies
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost
                | ForwardedHeaders.XForwardedPrefix,
        };
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

        if (!string.IsNullOrEmpty(_args.BasePath))
        {
            Console.WriteLine($"Using base path: {_args.BasePath}");
            app.UsePathBase(_args.BasePath);
        }

        app.UseRouting(); // First routing pass - match explicit routes (gRPC, controllers)
        app.UsePathToAppId(); // Rewrite path to appId if no endpoint matched
        app.UseRouting(); // Second routing pass - route the rewritten path
        app.UseCors();
        app.UseGrpcWeb();

        foreach (var mod in _appMods)
        {
            mod(app);
        }

        app.MapControllers();
        app.MapHub<AppHub>("/ivy/messages");
        app.MapHealthChecks("/ivy/health");
        app.MapGrpcService<DataTableService>().EnableGrpcWeb();

        app.UseFrontend(_args, logger2);
        app.UseAssets(_args, logger2, "Assets", "ivy/assets");

        return app;
    }

    public async Task RunAsync(CancellationTokenSource? cts = null)
    {
        var sessionStore = new AppSessionStore();

        cts ??= new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        if (!_args.Verbose)
        {
            // In production mode, prevent termination from unhandled exceptions
            AppDomain.CurrentDomain.SetData("HACK_SKIP_THROW_UNOBSERVED_TASK_EXCEPTIONS", true);
        }

        // Handle unobserved task exceptions to prevent process termination
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Console.WriteLine($@"[CRITICAL] Unobserved Task Exception: {e.Exception}");
            e.SetObserved(); // Prevents process termination
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = (Exception)e.ExceptionObject;
            Console.WriteLine($@"[CRITICAL] Unhandled Domain Exception - IsTerminating: {e.IsTerminating}");
            Console.WriteLine($@"[CRITICAL] Exception: {ex}");
        };

#if (DEBUG)
        // Run key listener on a dedicated thread to avoid consuming a ThreadPool worker
        _ = Task.Factory.StartNew(() =>
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Console.IsInputRedirected)
                    {
                        // Cannot read keys if input is redirected
                        Thread.Sleep(1000); // Check again later or just exit? Exit is safer.
                        break;
                    }

                    var key = Console.ReadKey(intercept: true);
                    if (key is { Modifiers: ConsoleModifiers.Control, Key: ConsoleKey.S })
                    {
                        sessionStore.Dump();
                    }
                }
            }
            catch (IOException ex)
            {
                // Console not available or detached
                Console.WriteLine($"[Warning] Debug key listener stopped: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Console not available
                Console.WriteLine($"[Warning] Debug key listener stopped: {ex.Message}");
            }
        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
#endif

        // CLI-only commands (--describe, --describe-connection, --test-connection) never start
        // the web host, so skip port checks entirely. Port 0 will be used below.
        if (!_args.IsCliCommand && ProcessHelper.IsPortInUse(_args.Port))
        {
            if (_args.IKillForThisPort)
            {
                ProcessHelper.KillProcessUsingPort(_args.Port);
            }
            else if (_args.FindAvailablePort)
            {
                var originalPort = _args.Port;
                var maxAttempts = 100;
                var attemptCount = 0;

                while (ProcessHelper.IsPortInUse(_args.Port) && attemptCount < maxAttempts)
                {
                    _args = _args with { Port = _args.Port + 1 };
                    attemptCount++;
                }

                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine($"\x1b[31mCould not find an available port after checking {maxAttempts} ports starting from {originalPort}.\x1b[0m");
                    return;
                }

                if (_args.Port != originalPort && !_args.Silent)
                {
                    Console.WriteLine($"\x1b[33mPort {originalPort} is in use. Using port {_args.Port} instead.\x1b[0m");
                }
            }
            else
            {
                Console.WriteLine($@"Port {_args.Port} is already in use on this machine.");

                Console.WriteLine(
                    "Specify a different port using '--port <number>', '--find-available-port', or '--i-kill-for-this-port' to just take it.");

                return;
            }
        }

        var app = BuildWebApplication(sessionStore, cts);
        if (app == null) return;

        if (_useHotReload)
        {
            HotReloadService.UpdateApplicationEvent += (types) =>
            {
                UpdateReservedPaths(app);
                AppRepository.Reload(_reservedPaths);
                var hubContext = app.Services.GetService<IHubContext<AppHub>>()!;
                hubContext.Clients.All.SendAsync("HotReload", cancellationToken: cts.Token);

                var themeService = app.Services.GetService<IThemeService>();
                if (themeService?.ThemeFactory != null)
                {
                    themeService.ReloadTheme();
                    var newCss = themeService.GenerateThemeCss();
                    hubContext.Clients.All.SendAsync("ApplyTheme", newCss, cancellationToken: cts.Token);
                }
            };
        }

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var url = app.Urls.FirstOrDefault() ?? "unknown";
            var port = new Uri(url).Port;
            var localUrl = $"http://localhost:{port}";
            if (!_args.Silent)
            {
                Console.WriteLine($@"Ivy is running on {localUrl} [{Process.GetCurrentProcess().Id}]. Press Ctrl+C to stop.");
            }
            if (_args.Browse)
            {
                ProcessHelper.OpenBrowser(localUrl);
            }
        });

        if (_args.Describe)
        {
            var description = ServerDescription.Gather(this, app.Services);
            Console.WriteLine(description.ToYaml());
            return;
        }

        if (_args.DescribeConnection != null)
        {
            var connection = ServerDescription.FindConnection(this, app.Services, _args.DescribeConnection);
            if (connection == null)
            {
                var available = ServerDescription.GetConnectionNames(this, app.Services);
                var availableList = available.Count > 0
                    ? string.Join(", ", available)
                    : "(none)";
                Console.Error.WriteLine($"Connection '{_args.DescribeConnection}' not found. Available connections: {availableList}");
                return;
            }

            var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();

            var connectionPath = Path.Combine(Directory.GetCurrentDirectory(), "Connections", connection.GetName());
            string? context = null;
            try { context = connection.GetContext(connectionPath); } catch { }

            var secrets = (connection is Ivy.IHaveSecrets hasSecrets)
                ? hasSecrets.GetSecrets().Select(s => s.Key).ToList()
                : new List<string>();

            var connectionDescription = new
            {
                name = connection.GetName(),
                type = connection.GetConnectionType(),
                @namespace = connection.GetNamespace(),
                context = string.IsNullOrEmpty(context) ? null : context,
                secrets,
                entities = connection.GetEntities().Select(e => e.Plural).ToList()
            };

            Console.WriteLine(serializer.Serialize(connectionDescription));
            return;
        }

        if (_args.TestConnection != null)
        {
            var connection = ServerDescription.FindConnection(this, app.Services, _args.TestConnection);
            if (connection == null)
            {
                var available = ServerDescription.GetConnectionNames(this, app.Services);
                var availableList = available.Count > 0
                    ? string.Join(", ", available)
                    : "(none)";
                Console.Error.WriteLine($"Connection '{_args.TestConnection}' not found. Available connections: {availableList}");
                return;
            }

            if (connection is Ivy.IHaveSecrets hasSecrets)
            {
                var config = app.Services.GetRequiredService<IConfiguration>();
                var missing = hasSecrets.GetSecrets()
                    .Where(s => !s.Optional && s.Preset == null && string.IsNullOrEmpty(config[s.Key]))
                    .Select(s => s.Key)
                    .ToList();

                if (missing.Count > 0)
                {
                    Console.Error.WriteLine($"Missing secrets: {string.Join(", ", missing)}");
                    return;
                }
            }

            var configuration = app.Services.GetRequiredService<IConfiguration>();
            var (ok, message) = await connection.TestConnection(configuration);

            if (ok)
            {
                Console.WriteLine($"OK: {message ?? "Connection successful."}");
            }
            else
            {
                Console.Error.WriteLine($"FAILED: {message ?? "Connection test failed."}");
            }
            return;
        }

        if (!CheckForMissingSecrets(app.Services))
            return;

        try
        {
            await app.StartAsync(cts.Token);
            await app.WaitForShutdownAsync(cts.Token);
        }
        catch (IOException)
        {
            Console.WriteLine($@"Failed to start Ivy server. Is the port already in use?");
        }
    }

    private bool CheckForMissingSecrets(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var missingByProvider = new Dictionary<string, List<string>>();

        // Gather from DI
        var providers = serviceProvider.GetServices<IHaveSecrets>();
        foreach (var provider in providers)
            CheckProvider(provider, config, missingByProvider);

        // Gather from assembly (fallback, skip already-found types)
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            var knownTypes = providers.Select(p => p.GetType()).ToHashSet();
            var types = assembly.GetLoadableTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false }
                            && typeof(IHaveSecrets).IsAssignableFrom(t)
                            && !knownTypes.Contains(t));

            foreach (var type in types)
            {
                try
                {
                    if (Activator.CreateInstance(type) is IHaveSecrets provider)
                        CheckProvider(provider, config, missingByProvider);
                }
                catch
                {
                    // Skip types that can't be instantiated
                }
            }
        }

        if (missingByProvider.Count == 0)
            return true;

        Console.Error.WriteLine("Missing secrets detected. The Ivy server cannot start.");
        Console.Error.WriteLine();
        foreach (var (providerName, keys) in missingByProvider)
        {
            Console.Error.WriteLine($"  {providerName}:");
            foreach (var key in keys)
                Console.Error.WriteLine($"    - {key}");
            Console.Error.WriteLine();
        }
        return false;
    }

    private static void CheckProvider(IHaveSecrets provider, IConfiguration config,
        Dictionary<string, List<string>> missingByProvider)
    {
        var missing = provider.GetSecrets()
            .Where(s => !s.Optional && s.Preset == null && string.IsNullOrEmpty(config[s.Key]))
            .Select(s => s.Key)
            .ToList();

        if (missing.Count > 0)
            missingByProvider[provider.GetType().Name] = missing;
    }

    private void UpdateReservedPaths(WebApplication app)
    {
        var actionDescriptorCollectionProvider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();

        // Use a local HashSet to collect paths (handles duplicates automatically)
        var reservedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Add existing reserved paths (from fluent API)
        reservedPaths.UnionWith(_fluentApiReservedPaths);

        // 2. Add auto-discovered controller routes
        foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
        {
            if (actionDescriptor.AttributeRouteInfo?.Template is { } template)
            {
                var segments = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var firstSegment = segments.FirstOrDefault();

                // Ignore dynamic segments (e.g. "{id}" or "user-{id}")
                if (!string.IsNullOrEmpty(firstSegment) && !firstSegment.Contains('{'))
                {
                    reservedPaths.Add("/" + firstSegment);
                }
            }
        }

        // 3. Add system excluded paths
        foreach (var path in AppRoutingHelpers.ExcludedPaths)
        {
            reservedPaths.Add(path.StartsWith('/') ? path : "/" + path);
        }

        // Atomically update the shared set (thread safety for startup)
        _reservedPaths = reservedPaths;
    }
}

public static class WebApplicationExtensions
{
    public static WebApplication UseFrontend(this WebApplication app, ServerArgs serverArgs, ILogger<Server> logger)
    {
        var assembly = typeof(WebApplicationExtensions).Assembly;
        var embeddedProvider = new EmbeddedFileProvider(
            assembly,
            $"{assembly.GetName().Name}"
        );
        var resourceName = $"{assembly.GetName().Name}.index.html";
        app.MapGet("/", async context =>
            await ServeIndexHtml(context, app, serverArgs, assembly, resourceName));

        // SPA fallback: serve index.html for any path not matched by other routes
        // (enables client-side routing for /sign-in, /foo/bar/sign-in, etc.)
        app.MapFallback(async context =>
            await ServeIndexHtml(context, app, serverArgs, assembly, resourceName));

        app.MapGet("/manifest.json", () =>
        {
            var manifest = app.Services.GetService<ManifestOptions>();
            if (manifest == null) return Results.NotFound();
            return Results.Json(manifest.ToManifest());
        });


        // In local development, prefer serving from the physical disk for faster updates and easier debugging
#if DEBUG
        try
        {
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "frontend", "dist");
            if (!Directory.Exists(physicalPath))
            {
                // Try cases where we are already in src or running from a sample project subfolder
                physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "frontend", "dist");
            }
            if (!Directory.Exists(physicalPath))
            {
                physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "frontend", "dist");
            }

            if (Directory.Exists(physicalPath))
            {
                logger.LogDebug("Serving frontend assets from physical path: {PhysicalPath}", Path.GetFullPath(physicalPath));
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(physicalPath)),
                    RequestPath = ""
                });
            }
            else
            {
                // Only log if CWD looks like it's inside the Ivy-Framework tree
                var cwd = Directory.GetCurrentDirectory();
                if (cwd.Contains("Ivy-Framework", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug("Frontend physical path not found: {PhysicalPath} (CWD: {Cwd})", Path.GetFullPath(physicalPath), cwd);
                }
            }
        }
        catch { /* fallback to embedded resources */ }
#endif

        app.UseStaticFiles(GetStaticFileOptions("", embeddedProvider, assembly));


        return app;
    }

    private static async Task ServeIndexHtml(HttpContext context, WebApplication app, ServerArgs serverArgs, Assembly assembly, string resourceName)
    {
        var version = assembly.GetName().Version?.ToString();
        if (!string.IsNullOrEmpty(version))
        {
            context.Response.Headers["ivy-version"] = version;
        }

        // Determine HTTP status code based on app routing
        var server = app.Services.GetRequiredService<Server>();
        var httpStatusCode = GetHttpStatusCodeForRequest(server, context);

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            var pipeline = new HtmlPipeline()
                .Use<LicenseFilter>()
                .Use<DevToolsFilter>()
                .Use<LocalFilesFilter>()
                .Use<MetaDescriptionFilter>()
                .Use<MetaGitHubUrlFilter>()
                .Use<TitleFilter>()
                .Use<ThemeFilter>()
                .Use<ManifestFilter>()
                .Use<OpenGraphFilter>()
                .Use<BasePathFilter>();

            foreach (var filter in server.GetCustomFilters())
                pipeline.Use(filter);

            server.GetPipelineConfigurator()?.Invoke(pipeline);

            var pipelineContext = new HtmlPipelineContext
            {
                Services = app.Services,
                ServerArgs = serverArgs
            };


            html = pipeline.Process(pipelineContext, html);

            context.Response.ContentType = "text/html";
            context.Response.StatusCode = httpStatusCode;
            var bytes = Encoding.UTF8.GetBytes(html);
            await context.Response.Body.WriteAsync(bytes);
        }
        else
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"Error: {resourceName} not found.");
        }
    }

    public static WebApplication UseAssets(this WebApplication app, ServerArgs args, ILogger<Server> logger,
        string folder, string? requestPath = null)
    {
        var assembly = args.AssetAssembly ?? Assembly.GetEntryAssembly()!;

        logger.LogDebug("Using {Assembly} for assets.", assembly.FullName);

        var embeddedProvider = new EmbeddedFileProvider(
            assembly,
            assembly.GetName().Name + "." + folder
        );

        var path = requestPath != null
            ? (requestPath.StartsWith("/") ? requestPath : "/" + requestPath)
            : "/" + folder;

        app.UseStaticFiles(GetStaticFileOptions(path, embeddedProvider, assembly));
        return app;
    }

    private static StaticFileOptions GetStaticFileOptions(string path, IFileProvider fileProvider, Assembly assembly)
    {
        return new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = path,
            OnPrepareResponse = ctx =>
            {
#if DEBUG
                ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                ctx.Context.Response.Headers.Pragma = "no-cache";
                ctx.Context.Response.Headers.Expires = "0";
#else
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(365),
                    MustRevalidate = true
                };
                var version = assembly.GetName().Version?.ToString() ?? "0.0.0.0";
                var lastWrite = "";

                // In single-file publishing, assembly.Location might be empty.
                // We fallback to just the version in those cases.
#pragma warning disable IL3000
                if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                {
                    lastWrite = File.GetLastWriteTimeUtc(assembly.Location).Ticks.ToString();
                }
#pragma warning restore IL3000

                ctx.Context.Response.Headers.ETag = version + ":" + lastWrite;
#endif
            }
        };
    }

    private static int GetHttpStatusCodeForRequest(Server server, HttpContext context)
    {
        var appRouter = new AppRouter(server);
        var routeResult = appRouter.Resolve(context);
        return routeResult.HttpStatusCode ?? 200;
    }
}