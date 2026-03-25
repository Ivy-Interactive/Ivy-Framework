using System.Collections.Immutable;
using System.Threading;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.ScrollText, group: ["Widgets", "Primitives"], searchHints: ["scroll", "chat", "stream", "rich text", "auto", "assistant"])]
public class AutoScrollApp : SampleBase
{
    protected override object? BuildSample()
    {
        var chatMessages = UseState(
            ImmutableArray.Create(
                new ChatMessage(
                    ChatSender.Assistant,
                    "Send a message to receive a long streaming rich-text reply. The chat list auto-scrolls as runs arrive.")));
        var ctsState = UseState<CancellationTokenSource?>(null);
        var isStreaming = UseState(false);
        var activeReplyId = UseRef(0);

        void OnChatSend(Event<Chat, string> e)
        {
            var trimmed = e.Value.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return;
            }

            ctsState.Value?.Cancel();
            var cts = new CancellationTokenSource();
            ctsState.Set(cts);
            isStreaming.Set(true);

            activeReplyId.Set(activeReplyId.Value + 1);
            var myId = activeReplyId.Value;

            void OnReplyFinished()
            {
                if (activeReplyId.Value == myId)
                {
                    isStreaming.Set(false);
                }
            }

            var withUser = chatMessages.Value.Add(new ChatMessage(ChatSender.User, trimmed));
            chatMessages.Set(withUser);
            chatMessages.Set(withUser.Add(new ChatMessage(
                ChatSender.Assistant,
                new StreamingRichAssistantReply(cts.Token, OnReplyFinished))));
        }

        void OnChatCancel(Event<Chat> _)
        {
            ctsState.Value?.Cancel();
            isStreaming.Set(false);
        }

        var chatPanel = new Chat(chatMessages.Value.ToArray(), OnChatSend, OnChatCancel)
            .Streaming(isStreaming.Value)
            .Placeholder("Ask anything — a long answer will stream token-by-token…")
            .Height(Size.Px(640))
            .Width(Size.Full());

        return Layout.Vertical().Gap(6)
               | Text.H1("AutoScroll")
               | Text.P(
                   "This sample focuses on chat: a RichTextBlock streams TextRun segments through UseStream while the message list stays pinned to the bottom. Cancel Request stops the writer; partial text stays visible.")

               | Callout.Info(
                   "Send a message and watch a long assistant reply stream in. Scroll up while it runs — auto-follow pauses until you return to the bottom, same as production chat.")

               | (new Card(
                   Layout.Vertical().Gap(2)
                   | Text.Muted(
                       "Assistant content uses Text.Rich() with UseStream<TextRun>() in a nested view (see RichTextBlock and Chat docs).")
                   | chatPanel
               ).Title("Streaming assistant"));
    }
}

/// <summary>
/// Owns one <see cref="IWriteStream{T}"/> per assistant message so streaming does not collide across turns.
/// </summary>
public sealed class StreamingRichAssistantReply : ViewBase
{
    private readonly CancellationToken _cancellationToken;
    private readonly Action _onComplete;

    public StreamingRichAssistantReply(CancellationToken cancellationToken, Action onComplete)
    {
        _cancellationToken = cancellationToken;
        _onComplete = onComplete;
    }

    public override object? Build()
    {
        var stream = UseStream<TextRun>();

        UseEffect(async () =>
        {
            try
            {
                await Task.Delay(400, _cancellationToken);
                await StreamLongEssayAsync(stream, _cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _onComplete();
            }
        }, OnMount());

        return Text.Rich()
            .Bold("Assistant")
            .Run(" — streaming", color: Colors.Muted)
            .LineBreak()
            .Italic("The chat panel auto-scrolls as these runs append.", color: Colors.Blue)
            .LineBreak()
            .UseStream(stream);
    }

    private static async Task StreamLongEssayAsync(IWriteStream<TextRun> stream, CancellationToken token)
    {
        var wordIndex = 0;
        var paragraphs = LongStreamEssay.Split("\n\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var p = 0; p < paragraphs.Length; p++)
        {
            token.ThrowIfCancellationRequested();
            if (p > 0)
            {
                stream.Write(new TextRun { LineBreak = true });
            }

            var paragraph = paragraphs[p];
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                token.ThrowIfCancellationRequested();
                var run = new TextRun(word) { Word = true };
                if (wordIndex % 28 == 0)
                {
                    run.Bold = true;
                }
                else if (wordIndex % 28 == 11)
                {
                    run.Italic = true;
                    run.Color = Colors.Teal;
                }

                stream.Write(run);
                wordIndex++;
                await Task.Delay(14, token);
            }
        }

        stream.Write(new TextRun { LineBreak = true });
        stream.Write(new TextRun("More in ") { Word = true });
        stream.Write(new TextRun("Ivy docs") { Word = true, Link = "https://docs.ivy.app", Color = Colors.Primary });
        stream.Write(new TextRun(".") { Word = true });
    }

    /// <summary>Very long multi-paragraph essay so the chat scroll region visibly tracks an extended stream.</summary>
    private static string LongStreamEssay =>
        """
        Declarative interfaces reward consistency. When the framework owns layout, typography, and motion, applications read as a family instead of a pile of one-off CSS. The author spends time on behavior and data, not on nudging pixels until midnight.

        Streaming responses change the rhythm of reading. Users tolerate latency when they see progress; they resent it when the screen stays frozen. Token-by-token rendering is not vanity — it is feedback that the system is still working.

        Auto-scroll is a promise: the newest information stays in view unless the reader deliberately moves away. That contract matters for logs, for chat, and for any surface where append-only content arrives from the network.

        Rich text lets a single assistant message carry emphasis, links, and tone without nesting dozens of widgets. Runs are small enough to stream cheaply yet expressive enough to feel crafted.

        Resilience sits beside presentation. Cancelling an in-flight stream should leave partial work visible and stop burning resources. The UI should never pretend completion when the server has already given up.

        Product teams rarely ask for “streaming” in isolation. They ask for answers that feel immediate, interfaces that feel alive, and failures that feel honest. Engineering translates those wishes into back-pressure, cancellation, and incremental rendering.

        A scrollable container is a contract with the human body. Eyes track the bottom when the mind expects novelty; the moment they scroll up, they are reading history, not watching the present. The UI must not fight that intent.

        Frameworks that hide layout behind sensible defaults free authors from debating spacing on every screen. The same defaults become a vocabulary: when every card uses the same radius, users learn faster and trust more.

        The hardest part of chat UX is not the bubble shape. It is the transition between waiting, partial output, and completion — including the awkward middle where the model is wrong but confident.

        Observability in the client means more than console logs. It means knowing which message was rendered, which stream was subscribed, and whether the last frame matched the last server intent.

        Accessibility is not a layer you paint on at the end. It is whether a screen reader can follow a stream without drowning in verbosity, and whether keyboard users can cancel without hunting for a tiny icon.

        Internationalization stretches every assumption about line breaks, emphasis, and reading order. A streaming run that works in English may need different spacing in Japanese; the pipeline has to stay flexible.

        Security matters even in read-only demos. Links must be validated, content must be escaped, and user-supplied text must never become script. Rich text is power; power needs boundaries.

        Performance is a feature. Re-rendering the entire transcript on every token is a recipe for jank. Incremental updates, keyed lists, and measured DOM work keep long conversations smooth.

        The network is not reliable. Retries, idempotency, and graceful degradation separate toys from tools. A chat that loses half a message on refresh teaches users not to trust it.

        State management is the hidden iceberg. Messages, drafts, typing indicators, read receipts, and stream handles all interact. The sample you are looking at is intentionally smaller than production — but the hooks are the same.

        Great samples teach one thing clearly. This one shows how long rich text can stream into a chat surface while the viewport follows along, until the reader chooses to look away.

        Design systems exist so that experimentation stays cheap. When a new widget lands, it should feel like it belongs — not like a visitor from another product.

        Documentation is part of the API. When a developer reads how to attach UseStream to RichTextBuilder, they should see the same mental model as the runtime, not a parallel universe.

        Feedback loops close when users can report what went wrong with enough context. The UI should surface message IDs, correlation tokens, or timestamps without turning every bubble into a spreadsheet.

        Finally, the goal is not to impress with length. The goal is to prove that the stack can carry a sustained stream without collapsing the frame rate or the reader’s patience. If you have read this far in the demo, the scroll mechanism has done its job.
        """;
}
