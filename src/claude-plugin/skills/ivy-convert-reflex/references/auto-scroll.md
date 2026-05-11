# Auto Scroll

A container div that automatically scrolls to the bottom when new content is added. Useful for chat interfaces, log viewers, or any dynamically updating feed where the most recent content should stay visible.

Reflex provides this as a generic container (`rx.auto_scroll`) that can wrap any content. Ivy does not have a standalone auto-scroll container; instead, auto-scroll behavior is built into the `Chat` widget, which covers the primary use case (chat interfaces).

## Reflex

```python
import reflex as rx

class AutoScrollState(rx.State):
    messages: list[str] = ["Initial message"]

    def add_message(self):
        self.messages.append(f"New message #{len(self.messages) + 1}")

def auto_scroll_example():
    return rx.vstack(
        rx.auto_scroll(
            rx.foreach(
                AutoScrollState.messages,
                lambda message: rx.box(
                    message,
                    padding="0.5em",
                    border_bottom="1px solid #eee",
                    width="100%",
                ),
            ),
            height="200px",
            width="300px",
            border="1px solid #ddd",
            border_radius="md",
        ),
        rx.button("Add Message", on_click=AutoScrollState.add_message),
        width="300px",
        align_items="center",
    )
```

## Ivy

```csharp
// Ivy has no standalone auto-scroll container.
// The Chat widget provides built-in auto-scroll for chat interfaces.
public class BasicChatDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Hello! I'm an echo bot.")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            var updated = messages.Value
                .Add(new ChatMessage(ChatSender.User, @event.Value))
                .Add(new ChatMessage(ChatSender.Assistant, $"You said: {@event.Value}"));
            messages.Set(updated);
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Parameters

| Parameter       | Documentation                                                                 | Ivy                                                        |
|-----------------|-------------------------------------------------------------------------------|------------------------------------------------------------|
| `height`        | Sets the height of the scrollable container                                   | `Chat.Height(Size)` sets the chat container height         |
| `width`         | Sets the width of the scrollable container                                    | `Chat.Width(Size)` sets the chat container width           |
| `padding`       | Adds padding inside the container                                             | Not directly on Chat; use inner layout widgets             |
| `border`        | Adds a border around the container                                            | Not supported (Chat has its own chrome)                    |
| `border_radius` | Rounds the corners of the container                                           | Not supported                                              |
| `overflow`      | Defaults to `"auto"` to enable scrolling                                      | Not supported (always auto within Chat)                    |
| `children`      | Any Reflex components — the container is generic                              | Chat only accepts `ChatMessage[]`                          |
| `placeholder`   | N/A                                                                           | `Chat.Placeholder(string)` sets input placeholder text     |
| `streaming`     | N/A                                                                           | `Chat.Streaming(bool)` controls cancel button visibility   |
| `on_send`       | N/A (auto_scroll has no send event; it is a passive container)                | `OnSend` event handler receives user messages              |
| `on_cancel`     | N/A                                                                           | `OnCancel` event handler for cancelling requests           |
