using System;
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
