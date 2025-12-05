using Ivy.Shared;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Chrome;
using Ivy.Connections;
using Ivy.Core;
using Ivy.Themes;
using Ivy.Middleware;
using Ivy.Views;
using Ivy.Views.DataTables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; //do not remove - used in RELEASE
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy;

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
    public string? MetaTitle { get; set; } = null;
    public string? MetaDescription { get; set; } = null;
    public Assembly? AssetAssembly { get; set; } = null;
#if DEBUG
    public bool FindAvailablePort { get; set; } = true;
#else
    public bool FindAvailablePort { get; set; } = false;
#endif
}

public class Server
{
    private IContentBuilder? _contentBuilder;
    private bool _useHotReload;
    private bool _useHttpRedirection;
    private readonly List<Action<WebApplicationBuilder>> _builderMods = new();
    private List<string> _reservedPaths = new();
    public IReadOnlyList<string> ReservedPaths => _reservedPaths;
    public string? DefaultAppId { get; private set; }
    public AppRepository AppRepository { get; } = new();
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IConfiguration Configuration { get; private set; } = ServerUtils.GetConfiguration();
    public Type? AuthProviderType { get; private set; } = null;
    public ServerArgs Args => _args;
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

        _args = _args with
        {
            AssetAssembly = _args.AssetAssembly ?? Assembly.GetCallingAssembly(),
        };

        Services.AddSingleton(_args);
        Services.AddSingleton(Configuration);

        AddDefaultApps();
    }

    private void AddDefaultApps()
    {
        this.UseErrorNotFound<NotFoundApp>();
    }

    public Server(FuncViewBuilder viewFactory) : this()
    {
        AddApp(new AppDescriptor
        {
            Id = AppIds.Default,
            Title = "Default",
            ViewFunc = viewFactory,
            Path = ["Apps"],
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

    public void AddConnectionsFromAssembly()
    {
        var assembly = Assembly.GetEntryAssembly();

        var connections = assembly!.GetTypes()
            .Where(t => t.IsClass && typeof(IConnection).IsAssignableFrom(t));

        foreach (var type in connections)
        {
            var connection = (IConnection)Activator.CreateInstance(type)!;
            connection.RegisterServices(this.Services);
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

    public Server UseConfiguration(IConfiguration configuration)
    {
        Configuration = configuration;
        return this;
    }

    public Server UseChrome(ChromeSettings settings)
    {
        return UseChrome(() => new DefaultSidebarChrome(settings));
    }

    public Server UseChrome<T>() where T : ViewBase
    {
        return UseChrome((() => (ViewBase)Activator.CreateInstance(typeof(T))!));
    }

    public Server UseChrome(Func<ViewBase>? viewFactory = null)
    {
        AddApp(new AppDescriptor
        {
            Id = AppIds.Chrome,
            Title = "Chrome",
            ViewFactory = viewFactory ?? (() => new DefaultSidebarChrome(ChromeSettings.Default())),
            Path = [],
            IsVisible = false
        });
        DefaultAppId = AppIds.Chrome;
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

        AddApp(new AppDescriptor
        {
            Id = AppIds.Auth,
            Title = "Auth",
            ViewFactory = viewFactory ?? (() => new DefaultAuthApp()),
            Path = [],
            IsVisible = false
        });
        AuthProviderType = typeof(T);
        return this;
    }

    public Server UseDefaultApp(Type appType)
    {
        DefaultAppId = AppHelpers.GetApp(appType).Id;
        return this;
    }

    public Server UseErrorNotFound<T>() where T : ViewBase
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
            Path = [],
            IsVisible = false
        });
        return this;
    }

    public Server UseBuilder(Action<WebApplicationBuilder> modify)
    {
        _builderMods.Add(modify);
        return this;
    }

    public Server ReservePaths(params string[] paths)
    {
        _reservedPaths.AddRange(paths);
        return this;
    }

    public Server SetMetaTitle(string title)
    {
        _args.MetaTitle = title;
        return this;
    }

    public Server SetMetaDescription(string description)
    {
        _args.MetaDescription = description;
        return this;
    }

    public Server UseTheme(Theme theme)
    {
        var themeService = new ThemeService();
        themeService.SetTheme(theme);
        Services.AddSingleton<IThemeService>(themeService);
        return this;
    }

    public Server UseTheme(Action<Theme> configureTheme)
    {
        var theme = new Theme();
        configureTheme(theme);
        var themeService = new ThemeService();
        themeService.SetTheme(theme);
        Services.AddSingleton<IThemeService>(themeService);
        return this;
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
            while (!cts.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(intercept: true);
                if (key is { Modifiers: ConsoleModifiers.Control, Key: ConsoleKey.S })
                {
                    sessionStore.Dump();
                }
            }
        }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        if (Utils.IsPortInUse(_args.Port))
        {
            if (_args.IKillForThisPort)
            {
                Utils.KillProcessUsingPort(_args.Port);
            }
            else if (_args.FindAvailablePort)
            {
                var originalPort = _args.Port;
                var maxAttempts = 100;
                var attemptCount = 0;

                while (Utils.IsPortInUse(_args.Port) && attemptCount < maxAttempts)
                {
                    _args = _args with { Port = _args.Port + 1 };
                    attemptCount++;
                }

                if (attemptCount >= maxAttempts)
                {
                    Console.WriteLine($@"[31mCould not find an available port after checking {maxAttempts} ports starting from {originalPort}.[0m");
                    return;
                }

                if (_args.Port != originalPort)
                {
                    Console.WriteLine($@"[33mPort {originalPort} is in use. Using port {_args.Port} instead.[0m");
                }
            }
            else
            {
                Console.WriteLine($@"[31mPort {_args.Port} is already in use on this machine.[0m");

                Console.WriteLine(
                    @"Specify a different port using '--port <number>' or '--i-kill-for-this-port' to just take it.");

                return;
            }
        }
#endif
        if (!string.IsNullOrEmpty(_args.DefaultAppId))
        {
            DefaultAppId = _args.DefaultAppId;
        }

        AppRepository.Reload();

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

        builder.WebHost.UseUrls($"http://*:{_args.Port}");

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = _args.Verbose;
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
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddHealthChecks();

        // Register theme service if not already registered
        if (Services.All(s => s.ServiceType != typeof(IThemeService)))
        {
            Services.AddSingleton<IThemeService, ThemeService>();
        }

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

        builder.Logging.SetMinimumLevel(!_args.Verbose ? LogLevel.Warning : LogLevel.Debug);

        var app = builder.Build();

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
                    });
                    await context.Response.WriteAsync(result);
                }
            });
        });

        if (_useHttpRedirection)
        {
            app.UseHttpsRedirection();
        }

        var logger = _args.Verbose ? app.Services.GetRequiredService<ILogger<Server>>() : new NullLogger<Server>();


        app.UsePathToAppId();

        app.UseRouting();
        app.UseCors();
        app.UseGrpcWeb();

        app.MapControllers();
        app.MapHub<AppHub>("/ivy/messages");
        app.MapHealthChecks("/ivy/health");
        app.MapGrpcService<DataTableService>().EnableGrpcWeb();

        if (_useHotReload)
        {
            HotReloadService.UpdateApplicationEvent += (types) =>
            {
                AppRepository.Reload();
                var hubContext = app.Services.GetService<IHubContext<AppHub>>()!;
                hubContext.Clients.All.SendAsync("HotReload", cancellationToken: cts.Token);
            };
        }

        app.UseFrontend(_args, logger);
        app.UseAssets(_args, logger, "Assets", "ivy/assets");

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var url = app.Urls.FirstOrDefault() ?? "unknown";
            var port = new Uri(url).Port;
            var localUrl = $"http://localhost:{port}";
            if (!_args.Silent)
            {
                Console.WriteLine($@"Ivy is running on {localUrl}. Press Ctrl+C to stop.");
            }
            if (_args.Browse)
            {
                Utils.OpenBrowser(localUrl);
            }
        });

        if (_args.Describe)
        {
            var description = ServerDescription.Gather(this, app.Services);
            Console.WriteLine(description.ToYaml());
            return;
        }

        try
        {
            CheckForAppIdCollisions(app);
            await app.StartAsync(cts.Token);
            await app.WaitForShutdownAsync(cts.Token);
        }
        catch (IOException)
        {
            Console.WriteLine($@"Failed to start Ivy server. Is the port already in use?");
        }
    }

    private void CheckForAppIdCollisions(WebApplication app)
    {
        var actionDescriptorCollectionProvider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();

        // Use a local HashSet to collect paths (handles duplicates automatically)
        var reservedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Add existing reserved paths (from fluent API)
        foreach (var path in _reservedPaths)
        {
            reservedPaths.Add(path);
        }

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
        foreach (var path in PathToAppIdMiddleware.ExcludedPaths)
        {
            reservedPaths.Add(path.StartsWith("/") ? path : "/" + path);
        }

        // Atomically update the shared list (thread safety for startup)
        _reservedPaths = reservedPaths.ToList();

        // 4. Check for collisions
        foreach (var appDescriptor in AppRepository.All())
        {
            var appIdPath = "/" + appDescriptor.Id;

            if (reservedPaths.Contains(appIdPath))
            {
                throw new InvalidOperationException($"App ID '{appDescriptor.Id}' collides with a reserved path '{appIdPath}'. Please choose a different App ID.");
            }
        }
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

                //Inject Ivy license:
                var configuration = app.Services.GetRequiredService<IConfiguration>();
                var ivyLicense = configuration["Ivy:License"] ?? "";
                if (!string.IsNullOrEmpty(ivyLicense))
                {
                    var ivyLicenseTag = $"<meta name=\"ivy-license\" content=\"{ivyLicense}\" />";
                    html = html.Replace("</head>", $"  {ivyLicenseTag}\n</head>");
                }
#if DEBUG
                var ivyLicensePublicKey = configuration["Ivy:LicensePublicKey"] ?? "";
                if (!string.IsNullOrEmpty(ivyLicensePublicKey))
                {
                    var ivyLicensePublicKeyTag =
                        $"<meta name=\"ivy-license-public-key\" content=\"{ivyLicensePublicKey}\" />";
                    html = html.Replace("</head>", $"  {ivyLicensePublicKeyTag}\n</head>");
                }
#endif

                //Inject Meta Title and Description
                if (!string.IsNullOrEmpty(serverArgs.MetaDescription))
                {
                    var metaDescriptionTag = $"<meta name=\"description\" content=\"{serverArgs.MetaDescription}\" />";
                    html = html.Replace("</head>", $"  {metaDescriptionTag}\n</head>");
                }

                if (!string.IsNullOrEmpty(serverArgs.MetaTitle))
                {
                    var metaTitleTag = $"<title>{serverArgs.MetaTitle}</title>";
                    html = Regex.Replace(html, "<title>.*?</title>", metaTitleTag, RegexOptions.Singleline);
                }

                // Inject theme configuration
                var themeService = app.Services.GetService<IThemeService>();
                if (themeService != null)
                {
                    var themeCss = themeService.GenerateThemeCss();
                    var themeMetaTag = themeService.GenerateThemeMetaTag();
                    html = html.Replace("</head>", $"  {themeMetaTag}\n  {themeCss}\n</head>");
                }

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
        });

        app.UseStaticFiles(GetStaticFileOptions("", embeddedProvider, assembly));

        return app;
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
                ctx.Context.Response.Headers.ETag = assembly.GetName().Version + ":" +
                                                    (!string.IsNullOrEmpty(assembly.Location) &&
                                                     File.Exists(assembly.Location)
                                                        ? File.GetLastWriteTimeUtc(assembly.Location).Ticks.ToString()
                                                        : "");
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
