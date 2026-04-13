using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Ivy.Benchmarks.E2E;

[TestFixture]
[NonParallelizable]
public class LatencyBenchmarkTests : PageTest
{
    private Process? _nativeProcess;
    private Process? _legacyProcess;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _nativeProcess = StartServer("Host.Native");
        _legacyProcess = StartServer("Host.Legacy");

        // Wait for both to boot
        using var client = new HttpClient();
        await WaitForServerAsync(client, "https://localhost:5010/hello");
        await WaitForServerAsync(client, "https://localhost:5011/hello");
    }

    private async Task WaitForServerAsync(HttpClient client, string url)
    {
        for (int i = 0; i < 30; i++)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode) return;
            }
            catch { }
            await Task.Delay(1000);
        }
        throw new Exception($"Server at {url} did not start in time.");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _nativeProcess?.Kill(true);
        _nativeProcess?.Dispose();

        _legacyProcess?.Kill(true);
        _legacyProcess?.Dispose();
    }

    private Process StartServer(string projectFolder)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run -c Release",
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "../../../", projectFolder),
                UseShellExecute = true
            }
        };
        proc.Start();
        return proc;
    }

    [Test]
    public async Task CompareWebSocketLatency_NativeVsLegacy()
    {
        var nativeLatency = await MeasureLatencyAsync("https://localhost:5010/hello");
        var legacyLatency = await MeasureLatencyAsync("https://localhost:5011/hello");

        TestContext.Progress.WriteLine($"--- E2E WebSocket Latency (JSON-Patch Render Cycle) ---");
        TestContext.Progress.WriteLine($"[1.2.27 Legacy] Typing 'x' took: {legacyLatency:F2} ms on average.");
        TestContext.Progress.WriteLine($"[Native-Ivy] Typing 'x' took: {nativeLatency:F2} ms on average.");
        TestContext.Progress.WriteLine($"Difference: {(legacyLatency - nativeLatency):F2} ms ({(1 - (nativeLatency / legacyLatency)) * 100:F1}% Faster)");
        TestContext.Progress.WriteLine($"-------------------------------------------------------");

        Assert.That(nativeLatency, Is.LessThan(legacyLatency), "Native Rust engine should be faster than Legacy engine.");
    }

    private async Task<double> MeasureLatencyAsync(string url)
    {
        var latencies = new List<double>();
        IWebSocket? ws = null;

        var runPage = await Context.NewPageAsync();

        runPage.WebSocket += (_, webSocket) =>
        {
            ws = webSocket;
        };

        await runPage.GotoAsync(url);

        // Wait for websocket to connect
        while (ws == null) { await Task.Delay(100); }
        var socket = ws!;

        long sentTime = 0;

#nullable disable
        socket.FrameSent += (_, frame) =>
        {
            var text = frame.Text;
            if (text != null && text.Contains("x"))
            {
                sentTime = Stopwatch.GetTimestamp();
            }
        };

        socket.FrameReceived += (_, frame) =>
        {
            var text = frame.Text;
            if (text != null && text.Contains("replace") && sentTime > 0)
            {
                var receiveTime = Stopwatch.GetTimestamp();
                var elapsedMs = (receiveTime - sentTime) / (double)Stopwatch.Frequency * 1000.0;
                latencies.Add(elapsedMs);
                sentTime = 0; // Reset
            }
        };
#nullable enable

        // Type into input natively!
        var input = runPage.Locator("input");
        await input.WaitForAsync();

        // Wait for SignalR to fully connect handshake
        await Task.Delay(1000);

        // Warmup: type 'a' to trigger JIT and FFI loading
        await input.FillAsync("a");
        await Task.Delay(500);

        // Reset metrics
        latencies.Clear();
        sentTime = 0;

        // Actual timed keystroke
        await input.FillAsync("x");

        // Let trailing frames settle
        await Task.Delay(1000);

        Assert.That(latencies, Is.Not.Empty, "No latency measurements were captured");

        await runPage.CloseAsync();

        return latencies.Average();
    }
}
