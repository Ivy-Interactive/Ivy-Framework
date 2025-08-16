using Ivy.Helpers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Chrome;
using Ivy.Connections;
using Ivy.Core;
using Ivy.DataTables;
using Ivy.Hooks;
using Ivy.Views;
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
    public string? MetaTitle { get; set; } = null;
    public string? MetaDescription { get; set; } = null;
    public Assembly? AssetAssembly { get; set; } = null;
}

public class Server
{
    private IContentBuilder? _contentBuilder;
    private bool _useHotReload;
    private bool _useHttpRedirection;
    private readonly List<Action<WebApplicationBuilder>> _builderMods = new();

    public string? DefaultAppId { get; private set; }
    public AppRepository AppRepository { get; } = new();
    public IServiceCollection Services { get; } = new ServiceCollection();
    public Type? AuthProviderType { get; private set; } = null;
    public ServerArgs Args => _args;

    private readonly ServerArgs _args;

    public Server(ServerArgs? args = null)
    {
        _args = args ?? IvyServerUtils.GetArgs();
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
    }

    public Server(FuncBuilder viewFactory) : this()
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

    public Server UseChrome(ChromeSettings settings)
    {
        return UseChrome(() => new DefaultSidebarChrome(settings));
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

    public Server UseBuilder(Action<WebApplicationBuilder> modify)
    {
        _builderMods.Add(modify);
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

    public async Task RunAsync(CancellationTokenSource? cts = null)
    {
        var sessionStore = new AppSessionStore();

        cts ??= new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

#if (DEBUG)
        _ = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(intercept: true);
                if (key is { Modifiers: ConsoleModifiers.Control, Key: ConsoleKey.S })
                {
                    sessionStore.Dump();
                }
            }
        }, cts.Token);

        if (Utils.IsPortInUse(_args.Port))
        {
            if (_args.IKillForThisPort)
            {
                Utils.KillProcessUsingPort(_args.Port);
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

        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddEnvironmentVariables();
        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            builder.Configuration.AddUserSecrets(entryAssembly);
        }

        foreach (var mod in _builderMods)
        {
            mod(builder);
        }

        builder.WebHost.UseUrls($"http://*:{_args.Port}");

        builder.Services.AddSignalR();
        builder.Services.AddSingleton(this);
        builder.Services.AddSingleton<IClientNotifier, ClientNotifier>();
        builder.Services.AddControllers()
            .AddApplicationPart(Assembly.Load("Ivy"))
            .AddControllersAsServices();
        builder.Services.AddGrpc();
        builder.Services.AddSingleton<IQueryableRegistry, QueryableRegistry>();
        builder.Services.AddSingleton<IContentBuilder>(_contentBuilder ?? new DefaultContentBuilder());
        builder.Services.AddSingleton(sessionStore);
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

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

        if (_useHttpRedirection)
        {
            app.UseHttpsRedirection();
        }

        var logger = _args.Verbose ? app.Services.GetRequiredService<ILogger<Server>>() : new NullLogger<Server>();

        app.UseRouting();
        app.UseCors();
        app.UseGrpcWeb();

        app.MapControllers();
        app.MapHub<AppHub>("/messages");
        app.MapGrpcService<Ivy.DataTables.DataTableService>().EnableGrpcWeb();

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
        app.UseAssets(_args, logger, "Assets");

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
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var html = await reader.ReadToEndAsync();

                //Inject IVY_LICENSE:
                var configuration = app.Services.GetRequiredService<IConfiguration>();
                var ivyLicense = configuration["IVY_LICENSE"] ?? "";
                if (!string.IsNullOrEmpty(ivyLicense))
                {
                    var ivyLicenseTag = $"<meta name=\"ivy-license\" content=\"{ivyLicense}\" />";
                    html = html.Replace("</head>", $"  {ivyLicenseTag}\n</head>");
                }
#if DEBUG
                var ivyLicensePublicKey = configuration["IVY_LICENSE_PUBLIC_KEY"] ?? "";
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

                context.Response.ContentType = "text/html";
                context.Response.StatusCode = 200;
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
        string folder)
    {
        var assembly = args.AssetAssembly ?? Assembly.GetEntryAssembly()!;

        logger.LogDebug("Using {Assembly} for assets.", assembly.FullName);

        var embeddedProvider = new EmbeddedFileProvider(
            assembly,
            assembly.GetName().Name + "." + folder
        );

        app.UseStaticFiles(GetStaticFileOptions("/" + folder, embeddedProvider, assembly));
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
}

public static class IvyServerUtils
{
    public static ServerArgs GetArgs()
    {
        var parser = new ArgsParser();
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        var parsedArgs = parser.Parse(args);
        return new ServerArgs()
        {
            Port = parser.GetValue(parsedArgs, "port", ServerArgs.DefaultPort),
            Verbose = parser.GetValue(parsedArgs, "verbose", false),
            IKillForThisPort = parser.GetValue(parsedArgs, "i-kill-for-this-port", false),
            Browse = parser.GetValue(parsedArgs, "browse", false),
            Args = parser.GetValue<string?>(parsedArgs, "args", null),
            DefaultAppId = parser.GetValue<string?>(parsedArgs, "app", null),
            Silent = parser.GetValue(parsedArgs, "silent", false)
        };
    }
}
