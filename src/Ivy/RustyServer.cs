using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Apps;
using Ivy.Core.Auth;
using Ivy.Core.ExternalWidgets;
using Ivy.Core.Server;
using Ivy.Core.Server.HtmlPipeline;
using Ivy.Core.Server.HtmlPipeline.Filters;
using Ivy.Core.Server.Middleware;
using Ivy.Themes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy;

/// <summary>
/// A new Rust-based server implementation designed to replace the original Server class.
/// Currently interoperates with a cdylib Rust library using FFI.
/// </summary>
public class RustyServer
{
    private const string RustLib = "rustserver";

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr rustserver_say_hello();

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void rustserver_free_string(IntPtr ptr);

    // Initial properties mirroring Server.cs
    public IReadOnlySet<string> ReservedPaths => throw new NotImplementedException("Rust port");
    public string? DefaultAppId { get; private set; }
    public AppRepository AppRepository { get; } = new();
    public NavigationBeaconRegistry NavigationBeaconRegistry { get; } = new();
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IConfiguration Configuration { get; private set; } = null!;
    public Type? AuthProviderType { get; private set; } = null;
    public ServerArgs Args { get; }

    public static Action<CookieOptions>? ConfigureAuthCookieOptions { get; set; }

    public RustyServer(ServerArgs? args = null)
    {
        Args = args ?? ServerUtils.GetArgs();
        
        // Example FFI test
        try
        {
            IntPtr ptr = rustserver_say_hello();
            if (ptr != IntPtr.Zero)
            {
                string? greeting = Marshal.PtrToStringAnsi(ptr);
                Console.WriteLine($"[RustyServer] Initialized: {greeting}");
                rustserver_free_string(ptr);
            }
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"[RustyServer] Failed to load rust lib: {ex.Message}");
        }
    }

    public RustyServer(FuncViewBuilder viewFactory) : this()
    {
    }

    public void AddApp<T>(bool isDefault = false) => throw new NotImplementedException("Rust port");
    public void AddApp(Type appType, bool isDefault = false) => throw new NotImplementedException("Rust port");
    public void AddApp(AppDescriptor appDescriptor) => throw new NotImplementedException("Rust port");
    public void AddAppsFromAssembly(Assembly? assembly = null) => throw new NotImplementedException("Rust port");
    public void AddConnectionsFromAssembly(Assembly? assembly = null) => throw new NotImplementedException("Rust port");
    public AppDescriptor GetApp(string id) => throw new NotImplementedException("Rust port");

    public RustyServer UseContentBuilder(IContentBuilder contentBuilder) => this;
    public RustyServer UseHotReload() => this;
    public RustyServer UseHttpRedirection() => this;
    public RustyServer UseCulture(string cultureName) => this;
    public RustyServer UseConfiguration(IConfiguration configuration) => this;
    public RustyServer UseConfiguration(Action<IConfigurationBuilder> configure) => this;
    
    public RustyServer UseAppShell(AppShellSettings settings) => this;
    public RustyServer UseAppShell<T>() where T : ViewBase, new() => this;
    public RustyServer UseAppShell(Func<ViewBase>? viewFactory = null) => this;
    
    public RustyServer UseAuth<T>(Action<T>? config = null, Func<ViewBase>? viewFactory = null) where T : class, IAuthProvider => this;
    public RustyServer RegisterAuthTokenHandler<T>(string provider) where T : class, IAuthTokenHandler => this;
    
    public RustyServer UseDefaultApp(Type appType) => this;
    public RustyServer UseErrorNotFound<T>() where T : ViewBase, new() => this;
    public RustyServer UseErrorNotFound(Func<ViewBase>? viewFactory = null) => this;
    
    public RustyServer UseWebApplicationBuilder(Action<WebApplicationBuilder> modify) => this;
    public RustyServer UseWebApplication(Action<WebApplication> modify) => this;
    
    public RustyServer ReservePaths(params string[] paths) => this;
    public RustyServer SetMetaTitle(string title) => this;
    public RustyServer SetMetaDescription(string description) => this;
    public RustyServer SetMetaGitHubUrl(string url) => this;
    
    public RustyServer UseTheme(Theme theme) => this;
    public RustyServer UseTheme(Func<Theme> themeFactory) => this;
    public RustyServer UseTheme(Action<Theme> configureTheme) => this;
    public RustyServer UseManifest(Action<ManifestOptions>? configure = null) => this;
    
    public RustyServer DangerouslyAllowLocalFiles() => this;
    public RustyServer UseHtmlFilter(IHtmlFilter filter) => this;
    public RustyServer UseHtmlPipeline(Action<HtmlPipeline> configure) => this;

    public async Task RunAsync(CancellationTokenSource? cts = null)
    {
        Console.WriteLine("[RustyServer] Handling state and lifecycle in Rust magically...");
        await Task.CompletedTask;
    }
}
