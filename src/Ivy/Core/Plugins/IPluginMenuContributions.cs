namespace Ivy.Core.Plugins;

/// <summary>
/// Exposes plugin-contributed menu transformers and badge providers to the app shell.
/// </summary>
public interface IPluginMenuContributions
{
    IReadOnlyList<Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>>> MenuTransformers { get; }
    IReadOnlyList<(string Tag, Func<IServiceProvider, int> CountProvider)> BadgeProviders { get; }
}
