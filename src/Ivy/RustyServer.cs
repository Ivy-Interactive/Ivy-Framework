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

[StructLayout(LayoutKind.Sequential)]
public struct CServerArgs
{
    public int Port;
    public int Verbose;
}

[StructLayout(LayoutKind.Sequential)]
public struct FfiWidgetProps
{
    public IntPtr Keys;   // *const *const c_char (String array pointer)
    public IntPtr Values; // *const *const c_char (String array pointer)
    public int Count;
}

[StructLayout(LayoutKind.Sequential)]
public struct FfiWidget
{
    public IntPtr Id;             // *const c_char
    public IntPtr ComponentType;  // *const c_char
    public int ParentIndex;       // i32
    public FfiWidgetProps Props;  // FfiWidgetProps
}

/// <summary>
/// A new Rust-based server implementation designed to replace the original Server class.
/// Currently interoperates with a cdylib Rust library using FFI.
/// </summary>
public class RustyServer : IDisposable
{
    private const string RustLib = "rustserver";

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr rustserver_create(ref CServerArgs args);

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void rustserver_run(IntPtr ptr);

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void rustserver_free(IntPtr ptr);

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void rustserver_render_json_tree(IntPtr state_ptr, IntPtr json_utf8_ptr, int json_len);

    private IntPtr _rustServerPtr = IntPtr.Zero;

    // A static channel to allow any C# WidgetTree diffing cycle to blindly push into the Rusty Server 
    // without needing complex decoupled DI refactors immediately.
    public static Action<byte[]>? GlobalRenderTree;

    // Internal hook to trigger the DOM diffing loop in Rust securely
    public void RenderFlatTree(byte[] utf8JsonTree)
    {
        if (_rustServerPtr == IntPtr.Zero || utf8JsonTree == null || utf8JsonTree.Length == 0) 
            return;
            
        // Pin the massive byte array safely so the GC doesn't move it while Rust reads it instantly
        var handle = GCHandle.Alloc(utf8JsonTree, GCHandleType.Pinned);
        try
        {
            rustserver_render_json_tree(_rustServerPtr, handle.AddrOfPinnedObject(), utf8JsonTree.Length);
        }
        finally
        {
            handle.Free();
        }
    }

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
        
        var cArgs = new CServerArgs
        {
            Port = Args.Port,
            Verbose = Args.Verbose ? 1 : 0
        };

        try
        {
            _rustServerPtr = rustserver_create(ref cArgs);
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"[RustyServer] Failed to load rust lib: {ex.Message}");
        }
        
        // Globally redirect all C# widget diffings directly into RustyServer's ultra-fast FFI sync
        GlobalRenderTree = RenderFlatTree;
    }

    public void Dispose()
    {
        if (_rustServerPtr != IntPtr.Zero)
        {
            rustserver_free(_rustServerPtr);
            _rustServerPtr = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }

    ~RustyServer()
    {
        Dispose();
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

    public Task RunAsync(CancellationTokenSource? cts = null)
    {
        if (_rustServerPtr != IntPtr.Zero)
        {
            // Spin up an example C# background task that simulates C# hook-based UI components 
            // constantly diffing and pushing their new states to the Rust engine!
            Task.Run(async () => {
                while(true) {
                    await Task.Delay(2000);
                    try {
                        var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new {
                            action = "full_render",
                            viewId = "main_app",
                            widgets = new[] { 
                                new { id = "btn1", type = "Button", label = $"Server Tick: {DateTime.Now.Second}" } 
                            }
                        });
                        this.RenderFlatTree(payload);
                    } catch (Exception e) {
                        Console.WriteLine($"[RustyServer] Simulation loop fault: {e.Message}");
                    }
                }
            });

            return Task.Run(() =>
            {
                rustserver_run(_rustServerPtr);
            });
        }
        else
        {
            Console.WriteLine("[RustyServer] Cannot run: Rust state failed to initialize.");
            return Task.CompletedTask;
        }
    }
}
