# Comment Thread

An interface to display and enter comments or messages within an application. Retool provides a dedicated `CommentThread` component for threaded comments with avatars, timestamps, and empty states. Ivy covers similar functionality through its `Chat` widget, which renders conversations between users and assistants using `ChatMessage` objects.

## Retool

```toolscript
// CommentThread with configuration
commentThread1.threadId = "thread-{{currentUser.id}}";
commentThread1.title = "Discussion";
commentThread1.placeholder = "Type a comment...";
commentThread1.showAvatar = true;
commentThread1.showTimestamp = true;

// Send a message
commentThread1.sendMessage();

// Set/clear values
commentThread1.setValue("Hello world");
commentThread1.clearValue();

// Visibility
commentThread1.setHidden(false);
commentThread1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

```csharp
public class CommentThreadDemo : ViewBase
{
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Welcome to the discussion!")
        ));

        void OnSend(Event<Chat, string> @event)
        {
            messages.Set(messages.Value.Add(
                new ChatMessage(ChatSender.User, @event.Value)));
        }

        return new Chat(messages.Value.ToArray(), OnSend)
            .Placeholder("Type a comment...")
            .Width(Size.Full())
            .Height(Size.Auto());
    }
}
```

## Parameters

| Parameter              | Retool Documentation                                      | Ivy                                                    |
|------------------------|-----------------------------------------------------------|--------------------------------------------------------|
| `threadId`             | Identifier for recording comments; supports dynamic values | Not supported (messages managed via state)             |
| `title`                | Title text displayed above the thread                     | Not supported                                          |
| `showTitle`            | Toggles title visibility                                  | Not supported                                          |
| `placeholder`          | Placeholder text for the input field                      | `.Placeholder(string)`                                 |
| `showAvatar`           | Displays user avatars next to messages                    | Not supported                                          |
| `avatarSrc`            | Image URL for the avatar                                  | Not supported                                          |
| `avatarIcon`           | Icon key for avatar display                               | Not supported                                          |
| `avatarFallback`       | Fallback text when no avatar image/icon exists            | Not supported                                          |
| `avatarImageSize`      | Size of the avatar image                                  | Not supported                                          |
| `showTimestamp`        | Displays message timestamps                               | Not supported                                          |
| `showEmptyState`       | Shows empty state before any messages are sent            | Not supported (initial messages set via state)         |
| `emptyTitle`           | Title text for the empty state                            | Not supported                                          |
| `emptyDescription`     | Description text for the empty state                      | Not supported                                          |
| `hidden`               | Controls component visibility                             | `Visible` (read-only property)                         |
| `isHiddenOnMobile`     | Hides on mobile layout                                    | Not supported                                          |
| `isHiddenOnDesktop`    | Hides on desktop layout                                   | Not supported                                          |
| `maintainSpaceWhenHidden` | Reserves space when hidden                             | Not supported                                          |
| `margin`               | External spacing around component                         | Not supported                                          |
| `style`                | Custom styling options                                    | Not supported                                          |
| `disableSubmit`        | Disables form submission                                  | Not supported                                          |
| `data`                 | Custom data storage on the component                      | Not supported                                          |
| `value`                | Current component value (read-only)                       | Messages managed via `ChatMessage[]` state             |
| `autoRefreshInterval`  | Frequency in seconds to reload data                       | Not supported                                          |
| `sendMessage()`        | Sends the current message                                 | `OnSend` event handler                                 |
| `setValue(value)`      | Sets the current value                                    | State managed via `UseState`                           |
| `clearValue()`         | Clears current values                                     | State managed via `UseState`                           |
| `resetValue()`         | Resets to default value                                   | State managed via `UseState`                           |
| `scrollIntoView()`     | Scrolls component into visible area                       | Not supported                                          |
| `setHidden(hidden)`    | Toggles component visibility                              | Not supported                                          |
| `exportData(options)`  | Exports data as CSV, TSV, JSON, or Excel                  | Not supported                                          |
| `Submit` event         | Triggered when a value is submitted                       | `OnSend` (`Func<Event<Chat, string>, ValueTask>`)     |
| `Click Action` event   | Triggered when action button is clicked                   | Not supported                                          |
| N/A                    | N/A                                                       | `OnCancel` - cancel ongoing operations                 |
| N/A                    | N/A                                                       | `.Streaming(bool)` - word-by-word streaming responses  |
| N/A                    | N/A                                                       | `.Width(Size)` / `.Height(Size)` - sizing              |
