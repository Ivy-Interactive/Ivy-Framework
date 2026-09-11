using Ivy.Desktop;

namespace Ivy.Test;

public class DesktopNavigationTests
{
    [Theory]
    [InlineData("http://localhost:5000", 5000, true)]
    [InlineData("http://localhost:5000/app/page", 5000, true)]
    [InlineData("https://localhost:5001", 5001, true)]
    [InlineData("http://127.0.0.1:5000", 5000, true)]
    [InlineData("http://127.0.0.1:5000/some/route?query=1", 5000, true)]
    [InlineData("https://127.0.0.1:8080", 8080, true)]
    [InlineData("http://[::1]:5000", 5000, true)]
    public void IsInternalUrl_LoopbackMatchingPort_ReturnsTrue(string url, int port, bool expected)
    {
        var uri = new Uri(url);
        var result = DesktopWindow.IsInternalUrl(uri, port);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://localhost:5173", 5000)]
    [InlineData("http://localhost:3000", 5000)]
    [InlineData("http://127.0.0.1:4200", 5000)]
    [InlineData("http://localhost:8080", 5000)]
    [InlineData("https://127.0.0.1:3000", 5001)]
    public void IsInternalUrl_LoopbackDifferentPort_ReturnsFalse(string url, int internalPort)
    {
        var uri = new Uri(url);
        var result = DesktopWindow.IsInternalUrl(uri, internalPort);
        Assert.False(result);
    }

    [Theory]
    [InlineData("https://github.com", 5000)]
    [InlineData("https://github.com/Ivy-Interactive/Ivy", 5000)]
    [InlineData("http://example.com", 5000)]
    [InlineData("http://example.com:5000", 5000)]
    public void IsInternalUrl_ExternalHosts_ReturnsFalse(string url, int internalPort)
    {
        var uri = new Uri(url);
        var result = DesktopWindow.IsInternalUrl(uri, internalPort);
        Assert.False(result);
    }

    [Theory]
    [InlineData("file:///path/to/file.html", 5000)]
    [InlineData("ws://localhost:5000", 5000)]
    [InlineData("wss://localhost:5000", 5000)]
    [InlineData("custom-scheme://localhost:5000", 5000)]
    public void IsInternalUrl_NonHttpOrHttpsSchemes_ReturnsFalse(string url, int internalPort)
    {
        var uri = new Uri(url);
        var result = DesktopWindow.IsInternalUrl(uri, internalPort);
        Assert.False(result);
    }
}
