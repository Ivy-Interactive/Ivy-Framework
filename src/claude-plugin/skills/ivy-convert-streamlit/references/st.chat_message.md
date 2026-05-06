# st.chat_message

Insert a chat message container. Displays a single message with an avatar indicating the author (user or assistant). The container can hold multiple Streamlit elements like text, charts, and images.

## Streamlit

```python
import streamlit as st

with st.chat_message("user"):
    st.write("Hello 👋")

message = st.chat_message("assistant")
message.write("Hello human")
message.bar_chart([1, 5, 2, 6])
```

## Ivy

Ivy does not have a standalone chat message container. Individual messages are represented as `ChatMessage` objects displayed within the `Chat` widget.

```csharp
var messages = UseState(ImmutableArray.Create<ChatMessage>(
    new ChatMessage(ChatSender.Assistant, "Hello! I'm an echo bot.")
));

void OnSend(Event<Chat, string> @event)
{
    var messagesWithUser = messages.Value.Add(
        new ChatMessage(ChatSender.User, @event.Value));
    messages.Set(messagesWithUser);

    var messagesWithAssistant = messagesWithUser.Add(
        new ChatMessage(ChatSender.Assistant, $"You said: {@event.Value}"));
    messages.Set(messagesWithAssistant);
}

return new Chat(messages.Value.ToArray(), OnSend);
```

## Parameters

| Parameter | Documentation                                                                                                                                                               | Ivy                                                                             |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| name      | The name of the message author. Can be `"user"`, `"assistant"`, `"ai"`, `"human"`, or a custom string. Enables preset styling and avatars for known author types.           | `ChatSender` enum (`ChatSender.User`, `ChatSender.Assistant`) in `ChatMessage`  |
| avatar    | The avatar shown next to the message. Accepts a single emoji character, a Material Symbol (e.g. `":material/icon_name:"`), `"spinner"`, an image, or `None` for automatic. | Not supported                                                                   |
