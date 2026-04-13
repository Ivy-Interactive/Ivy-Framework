using System.Collections.Concurrent;

namespace Ivy.Tendril.Services;

public class InboxWatcherService : IInboxWatcherService
{
    private readonly string _inboxPath;
    private readonly IJobService _jobService;
    private readonly Timer _pollTimer;
    private readonly ConcurrentDictionary<string, byte> _processing = new();
    private readonly FileSystemWatcher? _watcher;

    public InboxWatcherService(IConfigService config, IJobService jobService)
    {
        _jobService = jobService;
        _inboxPath = Path.Combine(config.TendrilHome, "Inbox");

        if (!Directory.Exists(_inboxPath))
            Directory.CreateDirectory(_inboxPath);

        // Recover crashed MakePlan jobs: rename .processing files back to .md
        RecoverProcessingFiles();

        ProcessExistingFiles();

        _watcher = new FileSystemWatcher(_inboxPath, "*.md")
        {
            InternalBufferSize = 65536,
            NotifyFilter = NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileCreated;
        _watcher.Error += (_, e) =>
            Program.WriteCrashLog($"[{DateTime.UtcNow:O}] InboxWatcher FSW error: {e.GetException()}");

        _pollTimer = new Timer(OnPollTimer, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public void Dispose()
    {
        _pollTimer.Dispose();
        _watcher?.Dispose();
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            _ = ProcessFileAsync(e.FullPath);
        }
        catch (Exception ex)
        {
            Program.WriteCrashLog($"[{DateTime.UtcNow:O}] InboxWatcher.OnFileCreated exception: {ex}");
        }
    }

    private void OnPollTimer(object? state)
    {
        try
        {
            ProcessExistingFiles();
        }
        catch (Exception ex)
        {
            Program.WriteCrashLog($"[{DateTime.UtcNow:O}] InboxWatcher.OnPollTimer exception: {ex}");
        }
    }

    internal void RecoverProcessingFiles()
    {
        if (!Directory.Exists(_inboxPath))
            return;

        foreach (var file in Directory.GetFiles(_inboxPath, "*.md.processing"))
            try
            {
                var mdPath = file[..^".processing".Length];
                if (File.Exists(mdPath))
                    // .md already exists — just delete the stale .processing file
                    File.Delete(file);
                else
                    File.Move(file, mdPath);
            }
            catch
            {
                Console.Error.WriteLine($"Failed to recover inbox file '{file}'. It will be retried on next startup.");
            }
    }

    internal void ProcessExistingFiles()
    {
        if (!Directory.Exists(_inboxPath))
            return;

        foreach (var file in Directory.GetFiles(_inboxPath, "*.md"))
            _ = ProcessFileAsync(file);
    }

    private async Task ProcessFileAsync(string filePath)
    {
        if (!_processing.TryAdd(filePath, 0))
            return;

        try
        {
            // Wait briefly for the file to be fully written
            await Task.Delay(500);

            if (!File.Exists(filePath))
                return;

            try
            {
                await ProcessInboxFileAsync(filePath);
            }
            catch (Exception ex)
            {
                // Retry once after a short delay
                await Task.Delay(1000);
                try
                {
                    await ProcessInboxFileAsync(filePath);
                }
                catch (Exception retryEx)
                {
                    Console.Error.WriteLine(
                        $"Failed to process inbox file '{filePath}' after retry. Initial error: {ex.Message}. Retry error: {retryEx.Message}");
                }
            }
        }
        finally
        {
            _processing.TryRemove(filePath, out _);
        }
    }

    private async Task ProcessInboxFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var content = await FileHelper.ReadAllTextAsync(filePath);
        var (project, description, sourcePath) = ParseContent(content);

        if (string.IsNullOrWhiteSpace(description))
        {
            Console.Error.WriteLine($"Skipping inbox file '{filePath}' — empty description.");
            return;
        }

        // Rename to .processing so the watcher/poller ignores it while the job runs
        var processingPath = filePath + ".processing";
        File.Move(filePath, processingPath);

        var args = new List<string> { "-Description", description, "-Project", project };
        if (!string.IsNullOrEmpty(sourcePath))
            args.AddRange(["-SourcePath", sourcePath]);
        _jobService.StartJob("MakePlan", args.ToArray(), processingPath);
    }

    internal static (string project, string description, string? sourcePath) ParseContent(string content)
    {
        if (content.StartsWith("---"))
        {
            var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
            if (endIndex > 3)
            {
                var frontmatter = content.Substring(3, endIndex - 3).Trim();
                var description = content.Substring(endIndex + 3).Trim();

                string? project = null;
                string? sourcePath = null;

                foreach (var line in frontmatter.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("project:", StringComparison.OrdinalIgnoreCase))
                        project = trimmed.Substring("project:".Length).Trim();
                    else if (trimmed.StartsWith("sourcePath:", StringComparison.OrdinalIgnoreCase))
                        sourcePath = trimmed.Substring("sourcePath:".Length).Trim();
                }

                return (project ?? "Auto", description, sourcePath);
            }
        }

        return ("Auto", content, null);
    }
}
