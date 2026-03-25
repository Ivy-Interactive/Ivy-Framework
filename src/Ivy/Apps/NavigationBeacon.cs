namespace Ivy;

/// <summary>
/// Describes how to navigate to an app for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type this beacon handles.</typeparam>
public record NavigationBeacon<TEntity>(
    string AppId,
    Func<TEntity, object> ArgsBuilder
);
