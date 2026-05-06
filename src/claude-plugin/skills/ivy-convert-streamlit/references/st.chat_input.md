# st.chat_input

Display a chat input widget that allows users to type and submit messages, optionally with file attachments and audio recordings.

## Streamlit

```python
prompt = st.chat_input("Say something")
if prompt:
    st.write(f"User has sent: {prompt}")
```

## Ivy

In Ivy, chat input is part of the `Chat` widget. User messages are captured through the `OnSend` event handler.

```csharp
var messages = UseState(ImmutableList<ChatMessage>.Empty);

return new Chat(messages.Value.ToArray(), OnSend)
    .Placeholder("Say something");

void OnSend(Event<Chat, string> @event)
{
    var userMessage = new ChatMessage(ChatSender.User, @event.Value);
    messages.Set(messages.Value.Add(userMessage));
}
```

## Parameters

| Parameter         | Documentation                                                                                          | Ivy                            |
|-------------------|--------------------------------------------------------------------------------------------------------|--------------------------------|
| placeholder       | Placeholder text shown when input is empty. Default: `"Your message"`                                  | `Placeholder(string)`          |
| max_chars         | Maximum number of characters allowed. `None` means no limit.                                           | Not supported                  |
| max_upload_size   | Maximum upload file size in MB. `None` uses server default.                                            | Not supported                  |
| accept_file       | Enable file attachments. `False`, `True`, `"multiple"`, or `"directory"`.                              | Not supported                  |
| file_type         | Allowed file extensions (e.g. `["jpg", "png"]`). `None` means all types.                               | Not supported                  |
| accept_audio      | Show an audio recording button. Default: `False`.                                                      | Not supported                  |
| audio_sample_rate | Sample rate in Hz for audio recording. Default: `16000`.                                               | Not supported                  |
| disabled          | Disable the input widget. Default: `False`.                                                            | Not supported                  |
| on_submit         | Callback invoked when the user submits a message.                                                      | `OnSend`                       |
