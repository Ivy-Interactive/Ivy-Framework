using System.ComponentModel;

namespace Ivy.Desktop;

public class DesktopSizeEventArgs(int width, int height) : EventArgs
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}

public class DesktopPointEventArgs(int x, int y) : EventArgs
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

public class DesktopNavigationEventArgs(string url) : CancelEventArgs
{
    public string Url { get; } = url;
}

public class DesktopPageLoadEventArgs(bool isStarted, string url) : EventArgs
{
    public bool IsStarted { get; } = isStarted;
    public bool IsFinished => !IsStarted;
    public string Url { get; } = url;
}

public record DesktopMonitorInfo(
    string? Name,
    int X,
    int Y,
    int Width,
    int Height,
    double ScaleFactor,
    bool IsPrimary
);

public record DesktopFileFilter(string Name, params string[] Extensions);
