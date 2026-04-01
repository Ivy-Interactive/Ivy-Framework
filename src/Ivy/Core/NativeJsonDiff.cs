using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace Ivy.Core;

/// <summary>
/// A zero-allocation, stateless FFI wrapper that calculates massive JSON-patch
/// differences purely using Rust cdylib math, destroying the C# execution bottlenecks!
/// </summary>
public static class NativeJsonDiff
{
    private const string RustLib = "rustserver";
    private static IntPtr _cachedHandle = IntPtr.Zero;

    static NativeJsonDiff()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeJsonDiff).Assembly, ResolveDllImport);
    }

    private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != RustLib) return IntPtr.Zero;
        if (_cachedHandle != IntPtr.Zero) return _cachedHandle;

        // 1. Try default load first (most common)
        if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle))
            return _cachedHandle = handle;

        // 2. High-performance probe: try the base directory directly for the specific filename
        // This handles cases where 'dotnet publish' moves the lib to the root.
        var libFileName = GetLibraryFileName();
        var baseLibPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, libFileName);
        if (File.Exists(baseLibPath) && NativeLibrary.TryLoad(baseLibPath, out handle))
            return _cachedHandle = handle;

        // 3. Fallback: full RID probe path (standard for NuGet runtimes layout)
        var probePath = GetProbePath();
        if (File.Exists(probePath) && NativeLibrary.TryLoad(probePath, out handle))
            return _cachedHandle = handle;

        return IntPtr.Zero;
    }

    private static string GetLibraryFileName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "rustserver.dll" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "librustserver.dylib" : "librustserver.so";

    private static string GetProbePath()
    {
        var rid = GetRuntimeIdentifier();
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", rid, "native", GetLibraryFileName());
    }

    private static string GetRuntimeIdentifier()
    {
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux";

        if (os == "linux" && (File.Exists("/lib/ld-musl-x86_64.so.1") || File.Exists("/lib/ld-musl-aarch64.so.1")))
        {
            os = "linux-musl";
        }

        return $"{os}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}";
    }

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr rustserver_diff_trees(
        IntPtr old_json_ptr, int old_len,
        IntPtr new_json_ptr, int new_len,
        out int out_len);

    [DllImport(RustLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void rustserver_free_string(IntPtr s);

    public static JsonNode? ComputePatch(byte[] oldTreeBytes, byte[] newTreeBytes)
    {
        if (oldTreeBytes == null || oldTreeBytes.Length == 0 || newTreeBytes == null || newTreeBytes.Length == 0)
            return null;

        var handleOld = GCHandle.Alloc(oldTreeBytes, GCHandleType.Pinned);
        var handleNew = GCHandle.Alloc(newTreeBytes, GCHandleType.Pinned);

        try
        {
            IntPtr cstr = rustserver_diff_trees(
                handleOld.AddrOfPinnedObject(), oldTreeBytes.Length,
                handleNew.AddrOfPinnedObject(), newTreeBytes.Length,
                out int patchLen
            );

            if (cstr == IntPtr.Zero || patchLen == 0)
                return null;

            // Instantly decode the math string back from Rust FFI memory
            string patchStr = Marshal.PtrToStringUTF8(cstr, patchLen);

            // Demand Rust drop the CString allocation frame-per-frame
            rustserver_free_string(cstr);

            return JsonNode.Parse(patchStr);
        }
        catch (DllNotFoundException ex)
        {
            var probePath = GetProbePath();
            throw new InvalidOperationException($"[NativeJsonDiff Error] Failed to load native library '{RustLib}'. \n" +
                $"RuntimeIdentifier: {GetRuntimeIdentifier()}\n" +
                $"Probed Path: {probePath}\n" +
                $"File Exists: {File.Exists(probePath)}\n" +
                $"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[NativeJsonDiff Error] Failed to compute patch from FFI layer.", ex);
        }
        finally
        {
            handleOld.Free();
            handleNew.Free();
        }
    }
}
