using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Widgets.Xterm;
using Terminal = Ivy.Widgets.Xterm.Terminal;

namespace XtermSamples.Apps;

// Demonstrates PtyOptions.CaptureOutput / PtyHandle.Output (chunk-boundary-safe UTF-8 decoding)
// and AnsiEscape.Strip, side by side with the live Terminal that already exercises pty.Stream.
[App(title: "Output Capture", icon: Icons.ScrollText, group: ["Terminal"], allowDuplicateTabs: true)]
class OutputCaptureApp : ViewBase
{
    public override object Build()
    {
        // IVYHOOK005: UsePty must be the first statement in Build(). A deliberately small
        // MaxCaptureLength makes the "keeps newest, drops oldest" trimming behavior visible
        // during a normal demo session instead of requiring megabytes of output.
        var pty = Context.UsePty(
            OperatingSystem.IsWindows() ? ["cmd"] : ["bash"],
            options: new PtyOptions { CaptureOutput = true, MaxCaptureLength = 4000 }
        );

        var transcript = UseState("");

        // PtyHandle is rebuilt every render, but the underlying process and decoder persist
        // across renders exactly as pty.Stream already does, so re-reading pty.Output on each
        // tick is safe and cheap.
        Context.UseInterval(
            () => transcript.Set(AnsiEscape.Strip(pty.Output)),
            TimeSpan.FromMilliseconds(500));

        return Layout.Vertical().Gap(2).Width(Size.Full()).Height(Size.Full())
            | Text.Muted("Type `echo héllo ✅ 🎉 ┌─┐` — the captured transcript on the right decodes correctly across chunk boundaries and strips ANSI.")
            | (Layout.Horizontal().Gap(4).Width(Size.Full()).Height(Size.Full())
                | new Terminal()
                    .Stream(pty.Stream)
                    .OnInput(pty.HandleInput)
                    .OnResize(pty.HandleResize)
                    .Closed(pty.Closed)
                    .AllowClipboard()
                    .Width(Size.Half())
                    .Height(Size.Full())
                | new CodeBlock(transcript.Value, Languages.Text)
                    .ShowCopyButton(true)
                    .Width(Size.Half())
                    .Height(Size.Full()));
    }
}
