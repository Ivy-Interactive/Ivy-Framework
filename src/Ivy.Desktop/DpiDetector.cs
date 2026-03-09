using System.Runtime.InteropServices;

namespace Ivy.Desktop;

public static class DpiDetector
{
    /// <summary>
    /// Gets the system scaling factor for high-DPI displays.
    /// Returns 1.0 for standard displays, 2.0 for retina/high-DPI displays, etc.
    /// </summary>
    public static double GetSystemScalingFactor()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetWindowsScalingFactor();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return GetMacOSScalingFactor();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetLinuxScalingFactor();
        }
        catch
        {
            // Fallback to standard scaling if detection fails
        }

        return 1.0; // Default scaling for standard displays
    }

    #region Windows DPI Detection

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    private const int LOGPIXELSX = 88;

    private static double GetWindowsScalingFactor()
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
        ReleaseDC(IntPtr.Zero, hdc);
        return dpiX / 96.0; // 96 DPI is 100% scaling (1.0x)
    }

    #endregion

    #region macOS Retina Detection

    [DllImport("libobjc.dylib")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("libobjc.dylib")]
    private static extern IntPtr sel_registerName(string selectorName);

    [DllImport("libobjc.dylib")]
    private static extern double objc_msgSend_fpret(IntPtr receiver, IntPtr selector);

    private static double GetMacOSScalingFactor()
    {
        // Get NSScreen main screen and its backingScaleFactor
        var nsScreenClass = objc_getClass("NSScreen");
        var mainScreenSelector = sel_registerName("mainScreen");
        var backingScaleFactorSelector = sel_registerName("backingScaleFactor");

        var mainScreen = objc_msgSend(nsScreenClass, mainScreenSelector);
        if (mainScreen == IntPtr.Zero)
            return 1.0;

        var scaleFactor = objc_msgSend_fpret(mainScreen, backingScaleFactorSelector);

        // Ensure we have a reasonable scaling factor
        return scaleFactor > 0 ? scaleFactor : 1.0;
    }

    #endregion

    #region Linux DPI Detection

    [DllImport("libX11", EntryPoint = "XOpenDisplay")]
    private static extern IntPtr XOpenDisplay(string? display_name);

    [DllImport("libX11", EntryPoint = "XDisplayWidth")]
    private static extern int XDisplayWidth(IntPtr display, int screen_number);

    [DllImport("libX11", EntryPoint = "XDisplayWidthMM")]
    private static extern int XDisplayWidthMM(IntPtr display, int screen_number);

    [DllImport("libX11", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplay(IntPtr display);

    private static double GetLinuxScalingFactor()
    {
        // First try environment variables (Wayland/modern desktops)
        var envScale = GetLinuxScalingFromEnvironment();
        if (envScale > 1.0)
            return envScale;

        // Fallback to X11 DPI calculation
        try
        {
            var display = XOpenDisplay(null);
            if (display == IntPtr.Zero)
                return 1.0;

            int widthPixels = XDisplayWidth(display, 0);
            int widthMM = XDisplayWidthMM(display, 0);
            XCloseDisplay(display);

            if (widthMM <= 0)
                return 1.0;

            double dpi = (widthPixels * 25.4) / widthMM; // Convert mm to inches, then get DPI
            return dpi / 96.0; // Convert DPI to scaling factor
        }
        catch
        {
            return 1.0;
        }
    }

    private static double GetLinuxScalingFromEnvironment()
    {
        // Check common scaling environment variables
        var scalingVars = new[] { "GDK_SCALE", "GDK_DPI_SCALE", "QT_SCALE_FACTOR" };

        foreach (var variable in scalingVars)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrEmpty(value) && double.TryParse(value, out var scale) && scale > 0)
                return scale;
        }

        return 1.0;
    }

    #endregion
}
