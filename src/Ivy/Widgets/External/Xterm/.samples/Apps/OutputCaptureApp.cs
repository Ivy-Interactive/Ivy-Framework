using System.Text.RegularExpressions;
using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Widgets.Xterm;
using Terminal = Ivy.Widgets.Xterm.Terminal;

namespace XtermSamples.Apps;

// Demonstrates PtyOptions.CaptureOutput / PtyHandle.Output (chunk-boundary-safe UTF-8 decoding)
// and AnsiEscape.Strip, side by side with the live Terminal that already exercises pty.Stream.
//
// The motivating real-world use case for CaptureOutput is server-side detection of URLs a hosted
// process prints to its console (e.g. "Ivy is running on https://localhost:5011"), so a caller
// can surface that link without the user having to click it inside the terminal itself (that
// client-side case is already covered by Terminal.OnLinkClick / WebLinksAddon). This sample
// regex-matches URLs out of the captured, ANSI-stripped transcript to prove that path works.
[App(title: "Output Capture", icon: Icons.ScrollText, group: ["Terminal"], allowDuplicateTabs: true)]
class OutputCaptureApp : ViewBase
{
    private static readonly Regex UrlPattern = new(@"https?://[^\s""'<>]+", RegexOptions.Compiled);

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

        // Recomputed fresh every render from the already-stripped transcript state, so no extra
        // hook/state is needed just to keep this in sync with `transcript`.
        var detectedUrls = UrlPattern.Matches(transcript.Value)
            .Select(m => m.Value.TrimEnd('.', ',', ')', ']', ':'))
            .Distinct()
            .ToArray();

        return Layout.Vertical().Gap(2).Width(Size.Full()).Height(Size.Full())
            | Text.Muted("Type `echo héllo ✅ 🎉 ┌─┐` — the captured transcript on the right decodes correctly across chunk boundaries and strips ANSI.")
            | Text.Muted("Type `echo Server running on https://localhost:5011` — the URL is detected below, proving console-printed URLs can be picked up server-side from PtyHandle.Output.")
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
                    .Height(Size.Full()))
            | Text.Muted("Detected URLs (regex-matched from the captured, ANSI-stripped transcript):")
            | (detectedUrls.Length == 0
                ? Text.Muted("None yet.")
                : Layout.Wrap(detectedUrls.Select(url => (object)new Button(url).Url(url).Link().OpenInNewTab())));
    }
}
