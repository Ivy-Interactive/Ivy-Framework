using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reflection;
using Ivy.Client;
using Ivy.Core;
using Path = System.IO.Path;

namespace Ivy.Chrome;

public static class IvyAgentServer
{
    public static async Task<IDisposable> StartAsync()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var sourceFolder = Path.GetFullPath(Path.Combine(currentFolder, "..", "..", ".."));

        //todo: ensure this is a proper Ivy application folder (maybe check for .ivy folder?)
        //todo: ensure ivy cli is installed and available in the PATH

        if (!await CheckIfAgentIsRunning(1))
        {
#if !DEBUG
        var arguments = $"\"{sourceFolder}\"";
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C start \"\" ivy start-ivy-agent-server {arguments}",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = false
        };
#endif

#if DEBUG
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"watch --ivy-folder \"{sourceFolder}\" --ivy-port 5001",
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = @"D:\Repos\_Ivy\Ivy\Ivy.Agent\"
            };
#endif

            var process = Process.Start(startInfo) ??
                throw new InvalidOperationException("Failed to start the Ivy Agent process");

            if (!await CheckIfAgentIsRunning(30))
            {
                throw new TimeoutException("Ivy Agent failed to start within timeout period.");
            }

            //return new ProcessKiller(process.Id); //todo:
            return Disposable.Empty;
        }

        return Disposable.Empty;
    }

    private static async Task<bool> CheckIfAgentIsRunning(int maxAttempts)
    {
        using var httpClient = new HttpClient();
        var delayMs = 250;

        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5001");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                if (i == maxAttempts - 1) return false;
                await Task.Delay(delayMs);
            }
        }

        return false;
    }

    private class ProcessKiller(int processId) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                var process = Process.GetProcessById(processId);
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                finally
                {
                    process.Dispose();
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}