using Ivy.IvyML.Studio.Helpers;

namespace Ivy.IvyML.Studio.Apps.Views;

/// <summary>
/// Shows the current wireframe rendered by a separate <c>ivyml run</c> process, embedded in an
/// iframe. The preview deliberately does not render in-process: Studio cannot reload the Ivy
/// assemblies it started with, so an in-process preview would keep showing the framework as it was
/// when Studio launched. See <see cref="PreviewServer"/>.
/// </summary>
public class PreviewView : ViewBase
{
    public override object Build()
    {
        var latest = Context.UseLatestWireframe();
        var preview = Context.UsePreviewServer();

        // Point the preview at the newest wireframe on mount, and whenever the agent writes a new
        // one or the code editor saves an edit.
        UseEffect(
            () => PreviewServer.Instance.SetWireframe(latest.Value.Path, latest.Value.Content),
            EffectTrigger.OnMount(), EffectTrigger.OnStateChange(latest));

        var status = preview.Value;

        var header = Layout.Horizontal().Gap(2)
            | Text.Muted(Describe(status))
            | new Button("Rebuild", () => PreviewServer.Instance.Rebuild(), ButtonVariant.Ghost)
                .Icon(Icons.RefreshCw)
                .Disabled(status.Phase is PreviewPhase.Building or PreviewPhase.Starting);

        var layout = new HeaderLayout(header, RenderBody(status));

        // HeaderLayout defaults to Scroll.Auto, which puts the body in a scroll area sized to its
        // content -- the iframe's Height=Full then has nothing to resolve against and collapses.
        // Scroll.None gives the body a full-height container instead and lets the iframe scroll its
        // own content. The other phases render short messages, so they keep the scrolling default.
        return status.Phase == PreviewPhase.Running ? layout.Scroll(Ivy.Scroll.None) : layout;
    }

    private static string Describe(PreviewStatus status) => status.Phase switch
    {
        PreviewPhase.Idle => "no wireframes yet",
        PreviewPhase.Building => "rebuilding ivyml...",
        PreviewPhase.Starting => "starting preview server...",
        PreviewPhase.Running => status.Url ?? "running",
        PreviewPhase.Failed => "preview failed",
        _ => "",
    };

    private static object RenderBody(PreviewStatus status)
    {
        if (status.Phase == PreviewPhase.Running && status.Url is { } url)
        {
            // RefreshToken remounts the frame on every restart; without it the browser would keep
            // showing the render from the process we just replaced.
            // The wrapping layout needs an explicit full height too: WithLayout() produces a plain
            // vertical stack that would otherwise size to its content and re-collapse the iframe.
            return Layout.Vertical(new Iframe(url, status.Generation).Width(Size.Full()).Height(Size.Full()))
                .Height(Size.Full())
                .Width(Size.Full())
                .RemoveParentPadding();
        }

        if (status.Phase == PreviewPhase.Failed)
        {
            return Layout.Vertical().Gap(2).Padding(4)
                | Text.Strong("Preview unavailable").Color(Colors.Red)
                | new CodeBlock(status.Message ?? "Unknown error.", Languages.Text).Width(Size.Full());
        }

        return Layout.Center().Height(Size.Full()).Width(Size.Full())
            | Text.Muted(status.Phase switch
            {
                PreviewPhase.Building => "Rebuilding ivyml with your latest framework changes...",
                PreviewPhase.Starting => "Starting the preview server...",
                _ => "Ask the agent to create a wireframe.",
            });
    }
}
