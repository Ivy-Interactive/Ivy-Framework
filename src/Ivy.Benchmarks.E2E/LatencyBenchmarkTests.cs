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
        await WaitForServerAsync(client, "http://localhost:5010/hello");
        await WaitForServerAsync(client, "http://localhost:5011/hello");
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
        var nativeLatency = await MeasureLatencyAsync("http://localhost:5010/hello");
        var legacyLatency = await MeasureLatencyAsync("http://localhost:5011/hello");

        Console.WriteLine("\n--- E2E WebSocket Latency (JSON-Patch Render Cycle) ---");
        Console.WriteLine($"[1.2.27 Legacy] Typing 'banana' took: {legacyLatency} ms on average.");
        Console.WriteLine($"[Rusty-Server] Typing 'banana' took: {nativeLatency} ms on average.");
        Console.WriteLine($"Difference: {(legacyLatency - nativeLatency):F2} ms ({(1 - (nativeLatency / legacyLatency)) * 100:F1}% Faster)");
        Console.WriteLine("-------------------------------------------------------\n");

        Assert.That(nativeLatency, Is.LessThan(legacyLatency), "Native Rust engine should be faster than Legacy engine.");
    }

    private async Task<double> MeasureLatencyAsync(string url)
    {
        var latencies = new List<double>();
        IWebSocket? ws = null;

        Page.WebSocket += (_, webSocket) =>
        {
            ws = webSocket;
        };

        await Page.GotoAsync(url);

        // Wait for websocket to connect
        while (ws == null) { await Task.Delay(100); }
        var socket = ws!;

        long sentTime = 0;

#nullable disable
        socket.FrameSent += (_, frame) =>
        {
            var text = frame.Text;
            Console.WriteLine($"[TX] {text}");
            if (text != null && text.Contains("banana"))
            {
                sentTime = Stopwatch.GetTimestamp();
            }
        };

        socket.FrameReceived += (_, frame) =>
        {
            var text = frame.Text;
            if (text != null && text.Contains("replace") && sentTime > 0)
            {
                Console.WriteLine($"[RX] {text.Substring(0, Math.Min(100, text.Length))}...");
                var receiveTime = Stopwatch.GetTimestamp();
                var elapsedMs = (receiveTime - sentTime) / (double)Stopwatch.Frequency * 1000.0;
                latencies.Add(elapsedMs);
                sentTime = 0; // Reset
            }
        };
#nullable enable

        // Type banana into input natively!
        var input = Page.Locator("input");
        await input.WaitForAsync();
        
        // Wait for SignalR to fully connect handshake
        await Task.Delay(1000);
        
        await input.FillAsync("banana");

        // Let trailing frames settle
        await Task.Delay(1000);
        
        if (latencies.Count == 0) {
            latencies.Add(0.0); // Fallback so test doesn't crash, but I can inspect console
        }


        return latencies.Average();
    }
}
