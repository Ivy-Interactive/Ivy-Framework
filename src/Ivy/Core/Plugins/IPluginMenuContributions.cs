namespace Ivy.Core.Plugins;

/// <summary>
/// Exposes plugin-contributed menu transformers to the app shell.
/// </summary>
public interface IPluginMenuContributions
{
    IReadOnlyList<Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>>> MenuTransformers { get; }
}
