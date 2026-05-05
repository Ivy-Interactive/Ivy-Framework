# LLM Chat

An interface for AI chat conversations. Displays a message thread between a user and an AI assistant, with an input field for sending new messages. Supports message history, streaming responses, and customizable assistant branding.

## Retool

```toolscript
// Configure the LLM Chat component in the inspector:
// - Set "Query to trigger" to a Retool AI query
// - Customize assistant name, avatar, and placeholder text

// Access chat data via JavaScript
{{ llmChat1.messageHistory }}
{{ llmChat1.lastMessage }}
{{ llmChat1.lastResponse }}

// Programmatic control
llmChat1.sendMessage();
llmChat1.clearHistory();
llmChat1.setValue("Hello, how can I help?");
llmChat1.exportData({ fileType: "json", fileName: "chat-export" });
```

## Ivy

```csharp
ChatMessage[] messages = [];

var chat = new Chat(messages, async e =>
{
    var userMessage = e.Value;
    messages = [..messages, new ChatMessage("user", userMessage)];

    // Call your AI service
    var response = await aiService.GetResponseAsync(userMessage);
    messages = [..messages, new ChatMessage("assistant", response)];
});

chat.Placeholder = "Ask me anything...";
chat.Streaming = true;
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| `assistantName` | Name of the AI assistant (default: "Retool AI") | Not supported (messages use role-based `ChatMessage`) |
| `avatarSrc` | Image URL for the assistant avatar | Not supported |
| `avatarIcon` | Icon key for the assistant avatar | Not supported |
| `avatarFallback` | Fallback text when no image/icon is set | Not supported |
| `placeholder` | Input field hint text | `Placeholder` property on `Chat` |
| `messageHistory` | Read-only array of sent/received messages | `ChatMessage[]` passed as constructor argument |
| `lastMessage` | Last message sent by the user (read-only) | Not supported (access via `OnSend` event value) |
| `lastResponse` | Last AI response (read-only) | Not supported (managed in user code) |
| `queryTargetId` | ID of query to send messageHistory to | Not supported (handled via `OnSend` callback) |
| `disableSubmit` | Disable form submission | Not supported |
| `emptyTitle` | Title before first message | Not supported |
| `emptyDescription` | Description before first message | Not supported |
| `showHeader` | Show/hide header area | Not supported |
| `showAvatar` | Show/hide assistant avatar | Not supported |
| `showTimestamp` | Show/hide message timestamps | Not supported |
| `showEmptyState` | Show empty state before first message | Not supported |
| `hidden` | Hide the component | `Visible` property on `Chat` |
| `streaming` | Enable streaming responses (via advanced settings) | `Streaming` property (controls cancel button visibility) |
| `title` | Header title text | Not supported |
| `style` | Custom style options | Not supported |
| `margin` | Outer margin spacing | Not supported |
| `data` | Custom data attached to component | Not supported |
| `Height` | Not a direct property (sized via layout) | `Height` property (`Size`) |
| `Width` | Not a direct property (sized via layout) | `Width` property (`Size`) |
| `Scale` | Not supported | `Scale` property |

### Methods

| Method | Retool | Ivy |
|--------|--------|-----|
| Send message | `llmChat.sendMessage()` | Handled via `OnSend` event callback |
| Clear history | `llmChat.clearHistory()` | Not supported (manage `ChatMessage[]` in user code) |
| Clear input | `llmChat.clearValue()` | Not supported |
| Set input value | `llmChat.setValue(value)` | Not supported |
| Export data | `llmChat.exportData(options)` | Not supported |
| Scroll into view | `llmChat.scrollIntoView(options)` | Not supported |
| Toggle visibility | `llmChat.setHidden(hidden)` | Set `Visible` property |
| Cancel request | Not supported | `OnCancel` event callback |

### Events

| Event | Retool | Ivy |
|-------|--------|-----|
| Message sent | Query trigger (configured via inspector) | `OnSend` callback (`Func<Event<Chat, string>, ValueTask>`) |
| Request cancelled | Not supported | `OnCancel` callback (`Func<Event<Chat>, ValueTask>`) |
