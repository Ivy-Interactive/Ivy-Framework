namespace Ivy.Tendril.Services;

public static class WorktreeValidationHelper
{
    public static bool IsWorktree(string path)
    {
        var gitPath = Path.Combine(path, ".git");
        if (!File.Exists(gitPath) || Directory.Exists(gitPath))
            return false;

        try
        {
            var content = File.ReadAllText(gitPath);
            return content.Contains("gitdir:");
        }
        catch
        {
            return false;
        }
    }
}
