namespace Ivy.Shared;

/// <summary>
/// Represents an internal navigation link to another app within the Ivy application.
/// </summary>
/// <param name="Title">The display title for the link.</param>
/// <param name="AppId">The unique identifier of the target app to navigate to.</param>
public record InternalLink(string Title, string AppId);