namespace Ivy.Tendril.Services;

public class InboxWatcherService : IDisposable
{
    private readonly JobService _jobService;
    private readonly FileSystemWatcher? _watcher;
    private readonly string _inboxPath;

    public InboxWatcherService(ConfigService config, JobService jobService)
    {
        _jobService = jobService;
        _inboxPath = Path.Combine(config.TendrilData, "Inbox");

        if (!Directory.Exists(_inboxPath))
            Directory.CreateDirectory(_inboxPath);

        // Process any files already in the inbox
        foreach (var file in Directory.GetFiles(_inboxPath, "*.md"))
        {
            _ = Task.Run(() => ProcessFileAsync(file));
        }

        _watcher = new FileSystemWatcher(_inboxPath, "*.md")
        {
            NotifyFilter = NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Created += (_, e) => _ = Task.Run(() => ProcessFileAsync(e.FullPath));
    }

    private async Task ProcessFileAsync(string filePath)
    {
        try
        {
            // Wait briefly for the file to be fully written
            await Task.Delay(500);

            if (!File.Exists(filePath))
                return;

            var content = await File.ReadAllTextAsync(filePath);
            var (project, description) = ParseContent(content);

            _jobService.StartJob("MakePlan", "-Description", description, "-Project", project);

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
                var (project, description) = ParseContent(content);

                _jobService.StartJob("MakePlan", "-Description", description, "-Project", project);

                File.Delete(filePath);
            }
            catch
            {
                // Give up — file will be picked up on next startup
            }
        }
    }

    internal static (string project, string description) ParseContent(string content)
    {
        if (content.StartsWith("---"))
        {
            var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
            if (endIndex > 3)
            {
                var frontmatter = content.Substring(3, endIndex - 3).Trim();
                var description = content.Substring(endIndex + 3).Trim();

                foreach (var line in frontmatter.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("project:", StringComparison.OrdinalIgnoreCase))
                    {
                        var project = trimmed.Substring("project:".Length).Trim();
                        return (project, string.IsNullOrEmpty(description) ? content : description);
                    }
                }

                // Frontmatter exists but no project field
                return ("[Auto]", string.IsNullOrEmpty(description) ? content : description);
            }
        }

        return ("[Auto]", content);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
