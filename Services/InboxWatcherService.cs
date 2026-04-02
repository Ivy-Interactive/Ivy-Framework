using System.Collections.Concurrent;

namespace Ivy.Tendril.Services;

public class InboxWatcherService : IDisposable
{
    private readonly JobService _jobService;
    private readonly FileSystemWatcher? _watcher;
    private readonly string _inboxPath;
    private readonly Timer _pollTimer;
    private readonly ConcurrentDictionary<string, byte> _processing = new();

    public InboxWatcherService(ConfigService config, JobService jobService)
    {
        _jobService = jobService;
        _inboxPath = Path.Combine(config.TendrilData, "Inbox");

        if (!Directory.Exists(_inboxPath))
            Directory.CreateDirectory(_inboxPath);

        ProcessExistingFiles();

        _watcher = new FileSystemWatcher(_inboxPath, "*.md")
        {
            NotifyFilter = NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Created += (_, e) => _ = Task.Run(() => ProcessFileAsync(e.FullPath));

        _pollTimer = new Timer(_ => ProcessExistingFiles(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    internal void ProcessExistingFiles()
    {
        if (!Directory.Exists(_inboxPath))
            return;

        foreach (var file in Directory.GetFiles(_inboxPath, "*.md"))
        {
            _ = Task.Run(() => ProcessFileAsync(file));
        }
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

            var content = await File.ReadAllTextAsync(filePath);
            var (project, description, sourcePath) = ParseContent(content);

            var args = new List<string> { "-Description", description, "-Project", project };
            if (!string.IsNullOrEmpty(sourcePath))
                args.AddRange(["-SourcePath", sourcePath]);
            _jobService.StartJob("MakePlan", args.ToArray());

            File.Delete(filePath);
        }
        catch
        {
            // Retry once after a short delay
            try
            {
                await Task.Delay(1000);
                if (!File.Exists(filePath))
                    return;

                var content = await File.ReadAllTextAsync(filePath);
                var (project, description, sourcePath) = ParseContent(content);

                var args = new List<string> { "-Description", description, "-Project", project };
                if (!string.IsNullOrEmpty(sourcePath))
                    args.AddRange(["-SourcePath", sourcePath]);
                _jobService.StartJob("MakePlan", args.ToArray());

                File.Delete(filePath);
            }
            catch
            {
                // Give up — file will be picked up on next poll
            }
        }
        finally
        {
            _processing.TryRemove(filePath, out _);
        }
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

                var desc = string.IsNullOrEmpty(description) ? content : description;
                return (project ?? "[Auto]", desc, sourcePath);
            }
        }

        return ("[Auto]", content, null);
    }

    public void Dispose()
    {
        _pollTimer.Dispose();
        _watcher?.Dispose();
    }
}
