using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;

internal class SourcePluginBuilder : IDisposable
{
    private static readonly HashSet<string> SourceExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".csproj", ".razor", ".props", ".targets"
    };

    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingBuilds = new();
    private readonly ConcurrentDictionary<string, DateTime> _buildCooldowns = new();
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(800);
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(1);
    private bool _disposed;

    public SourcePluginBuilder(ILogger logger)
    {
        _logger = logger;
    }

    public static bool IsSourcePlugin(string directory)
    {
        return Directory.Exists(directory) &&
               Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly).Any();
    }

    public static bool IsSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return SourceExtensions.Contains(ext);
    }

    public static bool BuildSync(string directory, ILogger logger, TimeSpan? timeout = null)
    {
        try
        {
            logger.LogInformation("Building source plugin: {Directory}", directory);

            using var process = StartBuildProcess(directory);
            if (process is null)
            {
                logger.LogError("Failed to start dotnet build process for {Directory}", directory);
                return false;
            }

            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(60);
            if (!process.WaitForExit(effectiveTimeout))
            {
                process.Kill();
                logger.LogError("dotnet build timed out for {Directory}", directory);
                return false;
            }

            if (process.ExitCode != 0)
            {
                var stderr = process.StandardError.ReadToEnd();
                var stdout = process.StandardOutput.ReadToEnd();
                logger.LogError("dotnet build failed for {Directory}:\n{Error}", directory,
                    string.IsNullOrWhiteSpace(stderr) ? stdout : stderr);
                return false;
            }

            logger.LogInformation("Source plugin built successfully: {Directory}", directory);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
    }

    public void ScheduleBuild(string pluginDirectory)
    {
        if (_disposed) return;

        if (_buildCooldowns.TryGetValue(pluginDirectory, out var cooldownUntil) && DateTime.UtcNow < cooldownUntil)
            return;

        if (_pendingBuilds.TryRemove(pluginDirectory, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        _ = Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource();
            _pendingBuilds[pluginDirectory] = cts;
            var token = cts.Token;

            try
            {
                await Task.Delay(_debounceDelay, token);

                if (_buildCooldowns.TryGetValue(pluginDirectory, out var cd) && DateTime.UtcNow < cd)
                    return;

                var success = await RunBuildAsync(pluginDirectory, _logger, TimeSpan.FromSeconds(60), token);

                if (!success)
                    _logger.LogError("Source plugin build failed: {Directory}", pluginDirectory);

                _buildCooldowns[pluginDirectory] = DateTime.UtcNow + _cooldownPeriod;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Build cancelled for {Directory}", pluginDirectory);
            }
            finally
            {
                _pendingBuilds.TryRemove(pluginDirectory, out _);
            }
        });
    }

    private static Process? StartBuildProcess(string directory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build -v quiet",
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            return Process.Start(psi);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException(
                $"The .NET SDK is required to build source plugins but was not found on this system. " +
                $"Install it from https://dotnet.microsoft.com/download or use a pre-built plugin instead.");
        }
    }

    private static async Task<bool> RunBuildAsync(string directory, ILogger logger, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Building source plugin: {Directory}", directory);

            using var process = StartBuildProcess(directory);
            if (process is null)
            {
                logger.LogError("Failed to start dotnet build process for {Directory}", directory);
                return false;
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            var stdout = await process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            var stderr = await process.StandardError.ReadToEndAsync(timeoutCts.Token);

            await process.WaitForExitAsync(timeoutCts.Token);

            if (process.ExitCode != 0)
            {
                logger.LogError("dotnet build failed for {Directory}:\n{Error}", directory,
                    string.IsNullOrWhiteSpace(stderr) ? stdout : stderr);
                return false;
            }

            logger.LogInformation("Source plugin built successfully: {Directory}", directory);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("dotnet build timed out for {Directory}", directory);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            logger.LogError(ex, "Exception running dotnet build for {Directory}", directory);
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var cts in _pendingBuilds.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _pendingBuilds.Clear();
    }
}
