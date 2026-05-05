# Agent Chat

A chat component that enables users to interact with an AI agent directly within an application. Supports sending messages, receiving responses, streaming output, loading states, and approval workflows for tool use.

## Retool

```toolscript
// Configure Agent Chat component in Retool
// The component is configured via the inspector and bound to a trigger query

// Send a message programmatically
agentChat.sendMessage('What is the weather in NYC?');

// Approve or reject tool use requests
agentChat.approveCurrentToolUse();
agentChat.rejectCurrentToolUse();

// Toggle visibility and header
agentChat.setHidden(false);
agentChat.setShowHeader(true);

// Read state
console.log(agentChat.messages);
console.log(agentChat.status); // "completed" | "in-progress" | "waiting_approval" | "error"
console.log(agentChat.isProcessing);
console.log(agentChat.messageDraft);
```

## Ivy

```csharp
public class AgentChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Hello! How can I help you?")
        ));

        var isStreaming = UseState(false);
        var ctsState = UseState<CancellationTokenSource?>(default(CancellationTokenSource?));

        void OnSend(Event<Chat, string> @event)
        {
            ctsState.Value?.Cancel();
            var cts = new CancellationTokenSource();
            ctsState.Set(cts);
            isStreaming.Set(true);

            var list = messages.Value.Add(new ChatMessage(ChatSender.User, @event.Value));
            var assistantIndex = list.Length;
            list = list.Add(new ChatMessage(ChatSender.Assistant, new ChatLoading()));
            messages.Set(list);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000, cts.Token);
                    var all = messages.Value.ToList();
                    all[assistantIndex] = new ChatMessage(ChatSender.Assistant,
                        $"Response to: '{@event.Value}'");
                    messages.Set(all.ToImmutableArray());
                }
                catch (OperationCanceledException)
                {
                    var all = messages.Value.ToList();
                    all[assistantIndex] = new ChatMessage(ChatSender.Assistant,
                        new Error("Cancelled", "The request was cancelled."));
                    messages.Set(all.ToImmutableArray());
                }
                finally
                {
                    isStreaming.Set(false);
                    ctsState.Set(default(CancellationTokenSource?));
                }
            });
        }

        void OnCancel(Event<Chat> _) => ctsState.Value?.Cancel();

        return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
            .Streaming(isStreaming.Value)
            .Placeholder("Type your message here...")
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| `agentId` | ID of the agent to invoke. Read-only string. | Not supported (agent logic is handled in user code via `OnSend`) |
| `agentInputs` | Inputs to send to the agent. Read-only string. | Not supported (inputs are managed in user code) |
| `messages` | Read-only array of sent/received messages. | `ChatMessage[]` passed as constructor argument |
| `messageDraft` | Read-only string of the current unsent message. | Not supported |
| `status` | Run status: `completed`, `in-progress`, `waiting_approval`, `error`. | Managed in user code; `ChatLoading`, `ChatStatus`, and `Error` widgets convey status |
| `isProcessing` | Read-only boolean indicating active processing. | `Streaming` property (bool) controls cancel button visibility |
| `queryTargetId` | ID of the query to trigger when agent is invoked. | Not supported (use `OnSend` event handler) |
| `title` | Header title text. | Not supported |
| `showHeader` | Toggle header visibility. Default `false`. | Not supported |
| `showBorder` | Toggle border display. Default `false`. | Not supported |
| `showEmptyState` | Display empty state before first message. | Not supported |
| `placeholder` | Not available as a property. | `Placeholder` (string) — input field hint text |
| `height` | Controlled via canvas layout. | `Height` (Size) — e.g. `Size.Auto()`, `Size.Full()` |
| `width` | Controlled via canvas layout. | `Width` (Size) — e.g. `Size.Full()`, `Size.Units(80)` |
| `visible` / `hidden` | `setHidden(bool)` method and `isHiddenOnDesktop`/`isHiddenOnMobile` properties. | `Visible` (bool) |
| `margin` | Margin around component. Default `4px 8px`. | Not supported (use layout wrappers) |
| `maintainSpaceWhenHidden` | Keep space when hidden. Default `false`. | Not supported |
| `sendMessage()` | Method to send a message to the agent. | `OnSend` event handler receives user messages |
| `approveCurrentToolUse()` | Approve pending tool execution. | Not supported |
| `rejectCurrentToolUse()` | Reject pending tool execution. | Not supported |
| `scrollIntoView()` | Scroll component into viewport. | Not supported |
| `onClose` | Event triggered when component closes. | Not supported |
| `onCancel` | Not available as an event. | `OnCancel` event handler for cancelling in-progress requests |
| `streaming` | Not available (agent manages its own streaming). | `Streaming` (bool) — controls cancel button visibility during streaming |
| `scale` | Not available. | `Scale` (Scale?) — optional scaling |
| Rich content | Not supported in agent responses. | Supports embedding any widget: buttons, cards, forms, charts, tables, code blocks |
