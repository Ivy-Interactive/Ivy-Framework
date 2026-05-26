namespace Ivy.Helpers;

internal static class ServerVersionHelper
{
    /// <summary>
    /// Returns a display label for the server version.
    /// On staging builds (assembly version = "0"), returns "Deployed MMM d · HH:mm UTC"
    /// using the current startup time. On release builds, returns "Version X.Y.Z".
    /// </summary>
    internal static string GetVersionLabel()
    {
        var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");

        if (version == "0")
        {
            var deployedAt = DateTime.UtcNow.ToString("MMM d · HH:mm UTC");
            return $"Deployed {deployedAt}";
        }

        return $"Version {version}";
    }
}
